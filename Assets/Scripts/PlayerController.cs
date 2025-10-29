using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    Statboard stats;
    
    [Header("Movement")]
    public float moveSpeed = 7f;
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
    }

    void Update()
    {
        HandleLook();
        HandleInput();
        HandleGrounding();
        HandleMovement();
        HandleJump();
        
        if (Input.GetMouseButtonDown(0)) {
            Physics.Raycast(cameraHolder.position, cameraHolder.forward * 1000f, out RaycastHit hitInfo, 1000, enemyMask);

            if (hitInfo.collider && hitInfo.collider.TryGetComponent(out Statboard stats)) {
                Debug.LogWarning("Hit: " + hitInfo.collider.gameObject.name);
                DamageInfo damageInfo = new DamageInfo(damage, this.stats);
                damageInfo.hitPoint = hitInfo.point;
                damageInfo.direction = (hitInfo.point - cameraHolder.position).normalized;
                damageInfo.statusEffects.Add(effect, 3);
                
                stats.health.TakeDamage(damageInfo);
            }
        }
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        moveInput = (transform.right * x + transform.forward * z).normalized;

        if (Input.GetButtonDown("Jump"))
            jumpQueued = true;
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