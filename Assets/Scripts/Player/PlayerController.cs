using System;
using ObjectPooling;
using Stats;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Statboard statboard;

    private InputManager inputs;

    private LimbManager limbManager;
    
    [Header("Movement")]
    public float moveSpeed => statboard.moveSpeed;
    public float acceleration = 25f;
    public float deceleration = 20f;

    [Header("Jumping & Gravity")]
    public float jumpHeight = 1.6f;
    public float gravity = -35f;
    public float terminalVelocity = -60f;
    public float coyoteTime = 0.12f;

    [Header("Camera")]
    public Transform cameraHolder;
    public float verticalClamp = 85f;
    public bool lockCursor = true;
    
    [Header("Testing")] 
    [SerializeField] private Damage damage;
    [SerializeField] private StatusEffectData effect;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private LayerMask interactionMask;

    private CharacterController controller;
    private Vector3 velocity;          // full 3D velocity (x,z for horizontal, y for vertical)
    private Vector3 moveInput;
    private bool jumpQueued;
    private float lastGroundedTime;
    private float xRotation;
    
    void Awake() {
        statboard = GetComponent<Statboard>();
        
        controller = GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        inputs = GetComponent<InputManager>();
        
        limbManager = GetComponent<LimbManager>();
    }

    private void Start() {
        statboard.eventManager.OnReceivedYourDamage += SpawnDamageNumber;
        statboard.eventManager.OnHealthChanged += PlayerHealthChanged;
        statboard.eventManager.OnManaChanged += PlayerManaChanged;
        statboard.eventManager.OnOrganChanged += PlayerOrganChanged;
        statboard.eventManager.OnRelicAdded += PlayerRelicAdded;
        statboard.eventManager.OnRelicRemoved += PlayerRelicRemoved;
        statboard.eventManager.OnLimbChanged += PlayerLimbChanged;

        foreach (Organ organ in statboard.organManager.organs) {
            PlayerOrganChanged(organ.data, null);
        }

        for (int i = 0; i < limbManager.limbs.Length; i++) {
            Limb limb = limbManager.limbs[i];
            if (i == 0 || i == 2) {
                PlayerLimbChanged(limb.data, LimbSide.Left, null);
            }
            else {
                PlayerLimbChanged(limb.data, LimbSide.Right, null);
            }
        }

        PlayerHUDEvents.OnRegisterStatboard?.Invoke(statboard);
        PlayerHUDEvents.OnHealthChanged?.Invoke(statboard.health.CurrentHealth, statboard.maxHealth);
        PlayerHUDEvents.OnManaChanged?.Invoke(statboard.mana.CurrentMana, statboard.maxMana);
        
        PlayerHUDEvents.OnRecycleRequest += OnRecycleRequest;
        PlayerHUDEvents.OnLimbRecycleRequest += OnLimbRecycleRequest;
        
        inputs.Inventory.Event += InventoryDebugEvent;
    }

    private void OnLimbRecycleRequest(LimbData limb, LimbSide side) {
        if (limbManager.RemoveLimb(limb, side)) {
            GetComponent<CurrencyManager>().AddCurrency(Mathf.FloorToInt(limb.RecycleValue));
        }
    }

    private void OnRecycleRequest(ItemData item, int itemCount) {
        bool result = false;
        Type dataType = item.GetType();
        if (dataType == typeof(OrganData)) {
            result = GetComponent<OrganManager>().RemoveOrgan(((OrganData)item).type);
        }
        else if (dataType == typeof(RelicData)) {
            result = GetComponent<RelicManager>().RemoveRelic((RelicData)item);
        }

        if (result) {
            GetComponent<CurrencyManager>().AddCurrency(itemCount * Mathf.FloorToInt(item.RecycleValue));
        }
    }

    private void PlayerManaChanged(float current, float max) {
        PlayerHUDEvents.OnManaChanged?.Invoke(current, max);
    }

    private void PlayerHealthChanged(float current, float max) {
        PlayerHUDEvents.OnHealthChanged?.Invoke(current, max);
    }

    private void InventoryDebugEvent(InputEvent<bool> inputEvent) {
        if (inputEvent.Triggered) {
            CanvasManager.Instance.OpenMenu(MenuType.Inventory);
        }
    }

    private void PlayerLimbChanged(LimbData newLimb, LimbSide limbSide, LimbData oldLimb) {
        PlayerHUDEvents.OnUpdateLimb?.Invoke(newLimb, limbSide);
    }

    private void PlayerRelicAdded(RelicData relic) {
        PlayerHUDEvents.OnAddedRelic?.Invoke(relic);
    }
    
    private void PlayerRelicRemoved(RelicData relic) {
        PlayerHUDEvents.OnRemovedRelic?.Invoke(relic);
    }

    private void PlayerOrganChanged(OrganData newOrgan, OrganData oldOrgan) { 
        PlayerHUDEvents.OnUpdateOrgan?.Invoke(newOrgan);
    }
    
    private void OnDestroy() {
        statboard.eventManager.OnReceivedYourDamage -= SpawnDamageNumber;
        statboard.eventManager.OnHealthChanged -= PlayerHealthChanged;
        statboard.eventManager.OnOrganChanged -= PlayerOrganChanged;
        statboard.eventManager.OnRelicAdded -= PlayerRelicAdded;
        statboard.eventManager.OnLimbChanged -= PlayerLimbChanged;
    }

    public void SpawnDamageNumber(DamageInfo damageInfo, Statboard victim) {
        if (victim == statboard) return;
        if (ObjectPool.TryPull(out DamageNumber damageNumber, damageInfo.hitPoint, transform.rotation)) {
            damageNumber.SetDamage(damageInfo);
        }
    }

    void Update() {
        HandleInput();
        HandleGrounding();
        HandleMovement();
        HandleJump();
        
        //Interaction
        Physics.Raycast(cameraHolder.position, cameraHolder.forward * 5f, out RaycastHit hit, 5f, interactionMask);
        if (hit.collider && hit.collider.TryGetComponent(out IInteractable interactable)) {
            if (interactable != null) {
                if (interactable.HasAltInteraction) {

                    if (inputs.Interact.Context.performed && (inputs.PrimaryInteract.Triggered)) {
                        interactable.Interact(statboard);
                        limbManager.HandleInteractAnim();
                    }
                    else if (inputs.Interact.Context.performed && (inputs.SecondaryInteract.Triggered))
                    {
                        interactable.AltInteract(statboard);
                        limbManager.HandleInteractAnim();
                    }
                }
                else {
                    if (inputs.Interact.Triggered) {
                        interactable.Interact(statboard);
                        limbManager.HandleInteractAnim();
                    }
                }
                PlayerHUDEvents.OnSetInteractionText?.Invoke($"{interactable.InteractionName()}", interactable.HasAltInteraction);
            }
        }
        else {
            PlayerHUDEvents.OnSetInteractionText?.Invoke("", false);
        }
    }

    public void LateUpdate() {
        HandleLook();
    }

    void HandleLook() {
        Vector2 inputVector = inputs.Look;
        inputVector *= Time.deltaTime;

        xRotation -= inputVector.y;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * inputVector.x);
    }

    void HandleInput() {
        Vector2 inputVector = inputs.Move;
        moveInput = (transform.right * inputVector.x + transform.forward * inputVector.y).normalized;

        if (inputs.Jump) {
            jumpQueued = true;
        }
    }

    void HandleGrounding()
    {
        if (controller.isGrounded)
            lastGroundedTime = Time.time;

        // After moving, check if we are on a steep slope and should slip
        if (controller.isGrounded)
        {
            if (OnSteepSlope(out Vector3 slopeNormal))
            {
                // Cancel any fake grounded stickiness
                velocity.y = Mathf.Min(velocity.y, 0f);
                // Slide down the slope
                Vector3 slideDirection = new Vector3(slopeNormal.x, -slopeNormal.y, slopeNormal.z);
                controller.Move(slideDirection * Time.deltaTime * 4f); // tweak 4f for slide speed
            }
        }
    }

    // Helper method to detect steep surfaces
    bool OnSteepSlope(out Vector3 hitNormal)
    {
        hitNormal = Vector3.up;
        // Slightly extend the ray to catch contact points below capsule
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height / 2f + 0.3f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > controller.slopeLimit)
            {
                hitNormal = hit.normal;
                return true;
            }
        }
        return false;
    }

    void HandleMovement()
    {
        Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);
        Vector3 targetVelocity = moveInput * moveSpeed;

        // Apply acceleration or deceleration based on input
        if (moveInput.sqrMagnitude > 0.001f)
        {
            horizontalVel = Vector3.MoveTowards(horizontalVel, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // apply passive friction when idle
            horizontalVel = Vector3.MoveTowards(horizontalVel, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Update horizontal part of velocity
        velocity.x = horizontalVel.x;
        velocity.z = horizontalVel.z;

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        if (velocity.y < terminalVelocity)
            velocity.y = terminalVelocity;

        // Prevent sticking to ground
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        // Move and handle ceiling collisions
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
        if ((flags & CollisionFlags.Above) != 0 && velocity.y > 0f)
            velocity.y = 0f;
    }

    void HandleJump()
    {
        bool canJump = (Time.time - lastGroundedTime) <= coyoteTime;

        if (jumpQueued && canJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpQueued = false;
        }
        else if (jumpQueued && !canJump && Time.time - lastGroundedTime > 0.2f)
        {
            jumpQueued = false;
        }
    }
}