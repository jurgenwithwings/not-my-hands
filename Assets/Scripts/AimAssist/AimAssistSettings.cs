using UnityEngine;

//[CreateAssetMenu(fileName = "AimAssistSettings", menuName = "ScriptableObjects/AimAssistSettings")]
public class AimAssistSettings : ScriptableObject {
    [Header("General")]
    public LayerMask targetLayers;
    public float maxDistance = 40f;
    public float maxAngle = 8f;
    public AnimationCurve angleFalloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Friction")]
    public bool useFriction = true; 
    public float frictionStrength => Mathf.Lerp(1f, 0.3f, PlayerSettings.AimAssistStrength);

    /*[Header("Rotational Pull")]
    public bool useRotationalPull = true;
    public float rotationalStrength = 4f;

    [Header("Sticky Tracking")]
    public bool useStickyTracking = true;
    public float trackingStrength = 2f;*/
}