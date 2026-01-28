using UnityEngine;

public class PhysicalLoot : MonoBehaviour {
    [SerializeField] private GameObject lootHolder;
    
    [Tooltip("How fast it spins around the up axis (degrees per second).")]
    private float spinSpeed = 15f;

    [Tooltip("Maximum tilt in degrees around the right axis.")]
    private Vector2 maxTilt = new(30f, 22f);

    [Tooltip("How fast it tilts back and forth (oscillations per second).")]
    private Vector2 tiltSpeed = new(0.1f, 0.065f);

    private Vector2 tiltTimer;
    private Vector3 eulerAngles;
    private float oscillationTimer;

    void Spin() {
        // Spin steadily around the up axis
        lootHolder.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // Oscillate smoothly between -maxTilt and +maxTilt using a sine wave
        tiltTimer += Time.deltaTime * tiltSpeed * Mathf.PI * 2f;
        Vector2 tiltAngle = new Vector2(Mathf.Sin(tiltTimer.x) * maxTilt.x, Mathf.Sin(tiltTimer.y) * maxTilt.y);

        // Apply tilt around the local right axis
        lootHolder.transform.localRotation = Quaternion.Euler(tiltAngle.x, lootHolder.transform.localEulerAngles.y, tiltAngle.y);
    }

    private void Update() {
        Spin();
    }
}