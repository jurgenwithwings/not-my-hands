using UnityEngine;

public class PRelic : MonoBehaviour, IInteractable {
    public RelicData data;
    
    [SerializeField] GameObject relicObject;

    [Tooltip("How fast it spins around the up axis (degrees per second).")]
    public float spinSpeed = 30f;

    [Tooltip("Maximum tilt in degrees around the right axis.")]
    public Vector2 maxTilt = new(30f, 22f);

    [Tooltip("How fast it tilts back and forth (oscillations per second).")]
    public Vector2 tiltSpeed = new(0.1f, 0.065f);

    private Vector2 tiltTimer;
    private Vector3 eulerAngles;
    private float oscillationTimer;

    public string InteractionName() {
        return data.displayName;
    }

    public void PickUp(Statboard interactor) {
        interactor.relicManager.AddRelic(data.relicType, data);
        Destroy(gameObject);
    }

    void Update()
    {
        // Spin steadily around the up axis
        relicObject.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);

        // Oscillate smoothly between -maxTilt and +maxTilt using a sine wave
        tiltTimer += Time.deltaTime * tiltSpeed * Mathf.PI * 2f;
        Vector2 tiltAngle = new Vector2(Mathf.Sin(tiltTimer.x) * maxTilt.x, Mathf.Sin(tiltTimer.y) * maxTilt.y);

        // Apply tilt around the local right axis
        relicObject.transform.localRotation = Quaternion.Euler(tiltAngle.x, relicObject.transform.localEulerAngles.y, tiltAngle.y);
    }
}