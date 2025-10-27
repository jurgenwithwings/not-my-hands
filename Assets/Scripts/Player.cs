using Stats;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed => stats.MoveSpeed;
    [SerializeField] float jumpForce = 5f;

    [Header("Look")]
    [SerializeField] Transform playerCamera;
    [SerializeField] float lookSensitivity = 2f;
    [SerializeField] float maxLookAngle = 85f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.2f;
    [SerializeField] LayerMask groundMask;

    Rigidbody rb;
    Statboard stats;
    
    float pitch = 0f;
    bool grounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        stats = GetComponent<Statboard>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        // Mouse look
        float mouseX = Input.GetAxisRaw("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        if (playerCamera != null)
            playerCamera.localEulerAngles = new Vector3(pitch, 0f, 0f);

        // Ground check
        if (groundCheck != null)
            grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        else
            grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);

        // Jump
        if (Input.GetButtonDown("Jump") && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        }

        if (Input.GetKeyDown(KeyCode.F)) {
            stats.MoveSpeed.AddModifier(new Modifier(0.5f - 1, ModifierType.PercentAdd, this, 4));
        }
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        Vector3 targetVel = move.normalized * moveSpeed;

        // Preserve vertical velocity (gravity/jumps)
        Vector3 newVel = new Vector3(targetVel.x, rb.linearVelocity.y, targetVel.z);
        rb.linearVelocity = newVel;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
