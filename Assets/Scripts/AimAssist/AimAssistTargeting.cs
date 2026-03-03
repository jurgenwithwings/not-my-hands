using UnityEngine;

public class AimAssistTargeting {
    public Transform CurrentTarget { get; private set; }

    public void UpdateTarget(Camera cam) {
        CurrentTarget = null;

        Collider[] hits = Physics.OverlapSphere(cam.transform.position, GameConfig.Instance.aimAssistSettings.maxDistance, GameConfig.Instance.aimAssistSettings.targetLayers);

        float bestScore = 0f;

        foreach (var hit in hits) {
            Vector3 dir = (hit.transform.position - cam.transform.position).normalized;
            float angle = Vector3.Angle(cam.transform.forward, dir);

            if (angle > GameConfig.Instance.aimAssistSettings.maxAngle) {
                continue;
            }

            float angle01 = angle / GameConfig.Instance.aimAssistSettings.maxAngle;
            float angleScore = GameConfig.Instance.aimAssistSettings.angleFalloff.Evaluate(1f - angle01);

            float distance = Vector3.Distance(cam.transform.position, hit.transform.position);
            float distance01 = distance / GameConfig.Instance.aimAssistSettings.maxDistance;

            float score = angleScore * (1f - distance01);

            if (score > bestScore) {
                bestScore = score;
                CurrentTarget = hit.transform;
            }
        }
    }
}