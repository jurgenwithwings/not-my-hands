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
    public float mouseSensitivity = 120f;
    public float verticalClamp = 85f;
    public bool lockCursor = true;
    
    [Header("Testing")] 
    [SerializeField] private DamageInstance[] damage;
    [SerializeField] private ClassReference<StatusEffect> effect;
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

        stats.eventManager.OnReceivedYourDamage += SpawnDamageNumber;
        stats.eventManager.OnDamageTaken += PlayerTakenDamage;
        
        inputs = GetComponent<InputManager>();
    }

    private void PlayerTakenDamage(DamageInfo obj) {
        PlayerHUDEvents.OnHealthChanged?.Invoke(stats.health.CurrentHealth, stats.maxHealth);
    }

    private void OnDestroy() {
        stats.eventManager.OnReceivedYourDamage -= SpawnDamageNumber;
        stats.eventManager.OnDamageTaken -= PlayerTakenDamage;
        
        //playerControls.Disable();
    }

    public void SpawnDamageNumber(DamageInfo damageInfo, Statboard victim) {
        if (victim == stats) return;
        if (ObjectPool.TryPull(damageInfo.hitPoint, transform.rotation, out DamageNumber damageNumber)) {
            damageNumber.SetDamage(damageInfo.totalDamage);
        }
    }

    void Update() {
        HandleInput();
        HandleGrounding();
        HandleMovement();
        HandleJump();
        
        if (inputs.PrimaryFire.Triggered) {
            Physics.Raycast(cameraHolder.position, cameraHolder.forward * 1000f, out RaycastHit hitInfo, 1000, enemyMask);

            if (hitInfo.collider && hitInfo.collider.TryGetComponent(out Statboard stats)) {
                //Skewed random distribution to favor lower numbers
                /*float t = Mathf.Pow(Random.value, 1.25f);
                damage[0].amount = Mathf.Lerp(1, 1500000, t);*/

                damage[0].amount = 100;
                DamageInfo damageInfo = new(damage, this.stats) {
                    hitPoint = hitInfo.point,
                    direction = (hitInfo.point - cameraHolder.position).normalized
                };
                damageInfo.statusEffects.Add(effect, 3);
                
                this.stats.eventManager.OnPreSendDamage?.Invoke(damageInfo, stats, this.stats);
                
                stats.health.TakeDamage(damageInfo.Copy());
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            float t = Mathf.Pow(Random.value, 3f);
            damage[0].amount = Mathf.Lerp(0, 20, t);
            
            DamageInfo damageInfo = new(damage, stats) {
                hitPoint = transform.position,
                direction = (transform.position - cameraHolder.position).normalized,
                selfDamage = true
            };
            damageInfo.statusEffects.Add(effect, 3);
            stats.health.TakeDamage(damageInfo.Copy());
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            ObjectPool.InitialisePool<DamageNumber>(1);
        }

        if (Input.GetKeyDown(KeyCode.Z)) {
            int index = Random.Range(1, 6);
            switch (index) {
                case 1:
                    effect = typeof(Burn);
                    break;
                case 2:
                    effect = typeof(Freeze);
                    break;
                case 3:
                    effect = typeof(Poison);
                    break;
                case 4:
                    effect = typeof(Charged);
                    break;
                case 5:
                    effect = typeof(Judged);
                    break;
            }
        }
    }

    public void LateUpdate() {
        HandleLook();
        
        Physics.Raycast(cameraHolder.position, cameraHolder.forward * 5f, out RaycastHit hit, 5f);

        if (hit.collider && hit.collider.TryGetComponent(out IInteractable interactable)) {
            if (interactable != null) {
                PlayerHUDEvents.OnSetInteractionText?.Invoke($"to Pick Up {interactable.InteractionName()}");

                if (interactable.HasAltInteraction) {
                    PlayerHUDEvents.OnSetInteractionText?.Invoke($"to Pick Up {interactable.InteractionName()}\n" +
                                                                $"Press 'Q' to getout idk");
                    if (Input.GetKeyDown(KeyCode.Alpha4))
                    {
                        interactable.AltInteract(stats);
                    }
                }
                if (inputs.Interact.Triggered) {
                    interactable.Interact(stats);
                }
            }
        }
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
            //print("Jump Triggered");
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