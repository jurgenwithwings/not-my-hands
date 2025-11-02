using System.Collections.Generic;
using UnityEngine;

public class IKLookAt : MonoBehaviour
{
    [Header("Bones (Root → Head)")]
    [SerializeField] private List<Transform> bones = new();

    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Rotation Settings")] 
    [SerializeField] private float stopLookAngle = 100f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float horizontalLimit = 60f;
    [SerializeField] private float verticalLimit = 40f;

    [Tooltip("Overall influence (0–1). 1 = full strength, 0.5 = half influence.")]
    private float weight => 1f / bones.Count;

    private Quaternion[] initialLocalRotations;
    private Transform rootBone;

    void Start()
    {
        if (bones.Count == 0)
        {
            Debug.LogWarning("HeadLookIK: No bones assigned!");
            enabled = false;
            return;
        }

        rootBone = bones[0];
        initialLocalRotations = new Quaternion[bones.Count];
        for (int i = 0; i < bones.Count; i++)
            initialLocalRotations[i] = bones[i].localRotation;
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        // Direction from root to target in world space
        Vector3 targetDir = (target.position - rootBone.position).normalized;

        // Get direction of the head in world space
        Vector3 forwardDir = rootBone.forward;

        // Angle between forward and target (for clamping)
        float yaw = Mathf.Atan2(targetDir.x, targetDir.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Asin(targetDir.y) * Mathf.Rad2Deg;
        
        if (Mathf.Abs(yaw) > stopLookAngle + horizontalLimit || Mathf.Abs(pitch) > stopLookAngle + verticalLimit) {
            ResetToDefault();
            return;
        }

        yaw = Mathf.Clamp(yaw, -horizontalLimit, horizontalLimit);
        pitch = Mathf.Clamp(pitch, -verticalLimit, verticalLimit);

        Quaternion clampedRot = Quaternion.Euler(pitch, yaw, 0);

        // Gradually apply this rotation along the chain
        int count = bones.Count;
        for (int i = 0; i < count; i++)
        {
            Transform bone = bones[i];
            float t = (i + 1) / (float)count; // 0 → 1 influence up the chain

            Quaternion targetLocalRot =
                Quaternion.Slerp(Quaternion.identity, clampedRot, t * weight);

            // Combine with initial local rotation
            bone.localRotation = Quaternion.Slerp(
                bone.localRotation,
                initialLocalRotations[i] * targetLocalRot,
                Time.deltaTime * rotationSpeed
            );
        }
    }

    // Optional reset method if target is lost
    public void ResetToDefault()
    {
        for (int i = 0; i < bones.Count; i++)
            bones[i].localRotation = Quaternion.Slerp(
                bones[i].localRotation,
                initialLocalRotations[i],
                Time.deltaTime * rotationSpeed
            );
    }
}
