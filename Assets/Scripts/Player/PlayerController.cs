using System;
using ObjectPooling;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private Statboard stats;

    private InputManager inputs;
    
    [Header("Movement")]
    public float moveSpeed => stats.moveSpeed;
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
    [SerializeField] private DamageInstance[] damage;
    [SerializeField] private StatusEffectData effect;
    [SerializeField] private LayerMask enemyMask;

    private CharacterController controller;
    private Vector3 velocity;          // full 3D velocity (x,z for horizontal, y for vertical)
    private Vector3 moveInput;
    private bool jumpQueued;
    private float lastGroundedTime;
    private float xRotation;
    
    void Awake() {
        stats = GetComponent<Statboard>();
        
        controller = GetComponent<CharacterController>();
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        inputs = GetComponent<InputManager>();
    }

    private void Start() {
        stats.eventManager.OnReceivedYourDamage += SpawnDamageNumber;
        stats.eventManager.OnDamageTaken += PlayerTakenDamage;
        stats.eventManager.OnHealthChanged += PlayerHealthChanged;
        stats.eventManager.OnOrganChanged += PlayerOrganChanged;
        stats.eventManager.OnRelicAdded += PlayerRelicAdded;
        stats.eventManager.OnLimbChanged += PlayerLimbChanged;

        foreach (Organ organ in stats.organManager.organs) {
            PlayerOrganChanged(organ.data, null);
        }
        
        PlayerHUDEvents.OnRegisterStatboard?.Invoke(stats);
        PlayerHUDEvents.OnHealthChanged?.Invoke(stats.health.CurrentHealth, stats.maxHealth);
        
        inputs.Inventory.Event += InventoryDebugEvent;
    }

    private void PlayerHealthChanged(float arg1, float arg2) {
        PlayerHUDEvents.OnHealthChanged?.Invoke(arg1, arg2);
    }

    private void InventoryDebugEvent(InputEvent<bool> inputEvent) {
        if (inputEvent.Triggered) {
            PlayerHUDEvents.OnDoTheInventory.Invoke(true);
            inputs.EnableActionMap(InputMap.UI);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void PlayerLimbChanged(LimbData newLimb, LimbSide limbSide, LimbData oldLimb) {
        PlayerHUDEvents.OnUpdateLimb?.Invoke(newLimb, limbSide);
    }

    private void PlayerRelicAdded(RelicData relic) {
        PlayerHUDEvents.OnAddedRelic?.Invoke(relic);
    }

    private void PlayerOrganChanged(OrganData newOrgan, OrganData oldOrgan) { 
        PlayerHUDEvents.OnUpdateOrgan?.Invoke(newOrgan);
    }

    private void PlayerTakenDamage(DamageInfo obj) {
        PlayerHUDEvents.OnHealthChanged?.Invoke(stats.health.CurrentHealth, stats.maxHealth);
    }
    

    private void OnDestroy() {
        stats.eventManager.OnReceivedYourDamage -= SpawnDamageNumber;
        stats.eventManager.OnDamageTaken -= PlayerTakenDamage;
        stats.eventManager.OnHealthChanged -= PlayerHealthChanged;
        stats.eventManager.OnOrganChanged -= PlayerOrganChanged;
        stats.eventManager.OnRelicAdded -= PlayerRelicAdded;
        stats.eventManager.OnLimbChanged -= PlayerLimbChanged;
    }

    public void SpawnDamageNumber(DamageInfo damageInfo, Statboard victim) {
        if (victim == stats) return;
        if (ObjectPool.TryPull(damageInfo.hitPoint, transform.rotation, out DamageNumber damageNumber)) {
            damageNumber.SetDamage(damageInfo.finalDamage);
            //PlayerHUDEvents.DebugText($"Damage Dealt: {damageInfo.finalDamage}");
        }
    }

    void Update() {
        HandleInput();
        HandleGrounding();
        HandleMovement();
        HandleJump();
        
        //Debug Interaction
        Physics.Raycast(cameraHolder.position, cameraHolder.forward * 5f, out RaycastHit hit, 5f);
        if (hit.collider && hit.collider.TryGetComponent(out IInteractable interactable)) {
            if (interactable != null) {
                if (interactable.HasAltInteraction) {

                    if (inputs.Interact.Context.performed && (inputs.PrimaryInteract.Triggered)) {
                        interactable.Interact(stats);
                    }
                    else if (inputs.Interact.Context.performed && (inputs.SecondaryInteract.Triggered))
                    {
                        interactable.AltInteract(stats);
                    }
                }
                else {
                    if (inputs.Interact.Triggered) {
                        interactable.Interact(stats);
                    }
                }
                PlayerHUDEvents.OnSetInteractionText?.Invoke($"to pick up {interactable.InteractionName()}", interactable.HasAltInteraction);
            }
        }
        else {
            PlayerHUDEvents.OnSetInteractionText?.Invoke("", false);
        }
        
        //Debug Fire Ray
        if (inputs.PrimaryFire.Triggered) {
            Physics.Raycast(cameraHolder.position, cameraHolder.forward * 1000f, out RaycastHit hitInfo, 1000, enemyMask);

            if (hitInfo.collider && hitInfo.collider.TryGetComponent(out Statboard stats)) {

                damage[0] = new DamageInstance(DamageInstance.DamageType.Physical, 100);
                DamageInfo damageInfo = new(damage, this.stats) {
                    hitPoint = hitInfo.point,
                };
                damageInfo.statusEffects.Add(effect, 3);
                
                this.stats.eventManager.OnPreSendDamage?.Invoke(ref damageInfo, stats, this.stats);
                
                stats.health.TakeDamage(damageInfo.Copy());
            }
        }

        // Debug Take Damage
        if (Input.GetKeyDown(KeyCode.Q)) {
            PlayerHUDEvents.DebugText("Trying to take damage");
            float t = Mathf.Pow(Random.value, 3f);
            damage[0].baseAmount = Mathf.Lerp(0, 20, t);
            
            DamageInfo damageInfo = new(damage, stats) {
                hitPoint = transform.position,
                selfDamage = true
            };
            damageInfo.statusEffects.Add(effect, 3);
            stats.health.TakeDamage(damageInfo.Copy());
        }

        //Debug Swap Status Effect Type
        if (Input.GetKeyDown(KeyCode.Z)) {
            int index = Random.Range(1, 7);
            switch (index) {
                case 1:
                    effect = GameConfig.Instance.bleed;
                    break;
                case 2:
                    effect = GameConfig.Instance.burn;
                    break;
                case 3:
                    effect = GameConfig.Instance.freeze;
                    break;
                case 4:
                    effect = GameConfig.Instance.charged;
                    break;
                case 5:
                    effect = GameConfig.Instance.poison;
                    break;
                case 6:
                    effect = GameConfig.Instance.judged;
                    break;
            }
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