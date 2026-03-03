using System.Collections.Generic;
using UnityEngine;

public class AimAssistController {
    private AimAssistTargeting targeting = new();
    private List<IAimAssistModule> modules = new();

    public AimAssistController() {
        modules.Add(new AimAssistFriction());
        //modules.Add(new AimAssistRotationalPull());
        //modules.Add(new AimAssistStickyTracking());
    }

    public Vector2 Process(Vector2 rawInput, Camera cam, float deltaTime) {
        targeting.UpdateTarget(cam);
        Transform target = targeting.CurrentTarget;

        Vector2 modified = rawInput;

        foreach (var module in modules) {
            modified = module.Modify(modified, cam, target, deltaTime);
        }

        return modified;
    }
}

public interface IAimAssistModule {
    Vector2 Modify(Vector2 rawInput, Camera cam, Transform target, float deltaTime);
}

public class AimAssistFriction : IAimAssistModule {
    public Vector2 Modify(Vector2 rawInput, Camera cam, Transform target, float deltaTime) {
        if (!GameConfig.Instance.aimAssistSettings.useFriction || target == null) {
            return rawInput;
        }

        Vector3 dirToTarget = (target.position - cam.transform.position).normalized;
        float angle = Vector3.Angle(cam.transform.forward, dirToTarget);

        float t = 1f - Mathf.Clamp01(angle / GameConfig.Instance.aimAssistSettings.maxAngle);
        float slowdown = Mathf.Lerp(1f, GameConfig.Instance.aimAssistSettings.frictionStrength, t);

        return rawInput * slowdown;
    }
}

/*public class AimAssistRotationalPull : IAimAssistModule {
    public Vector2 Modify(Vector2 rawInput, Camera cam, Transform target, float deltaTime) {
        if (!GameConfig.Instance.aimAssistSettings.useRotationalPull || target == null) {
            return rawInput;
        }

        Vector3 toTarget = (target.position - cam.transform.position).normalized;

        float angle = Vector3.Angle(cam.transform.forward, toTarget);
        float t = 1f - Mathf.Clamp01(angle / GameConfig.Instance.aimAssistSettings.maxAngle);

        float strength = GameConfig.Instance.aimAssistSettings.rotationalStrength * t * deltaTime;

        Vector3 newForward = Vector3.Slerp(cam.transform.forward, toTarget, strength);

        Vector3 delta = Quaternion.FromToRotation(cam.transform.forward, newForward).eulerAngles;

        Vector2 assistInput = new Vector2(-delta.y, delta.x);

        return rawInput + assistInput;
    }
}

public class AimAssistStickyTracking : IAimAssistModule {
    public Vector2 Modify(Vector2 rawInput, Camera cam, Transform target, float deltaTime) {
        if (!GameConfig.Instance.aimAssistSettings.useStickyTracking || target == null) {
            return rawInput;
        }

        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null) {
            return rawInput;
        }

        Vector3 predictedPosition = target.position + rb.linearVelocity * 0.1f;

        Vector3 toPredicted = (predictedPosition - cam.transform.position).normalized;

        float angle = Vector3.Angle(cam.transform.forward, toPredicted);
        float t = 1f - Mathf.Clamp01(angle / GameConfig.Instance.aimAssistSettings.maxAngle);

        float strength = GameConfig.Instance.aimAssistSettings.trackingStrength * t * deltaTime;

        Vector3 newForward = Vector3.Slerp(cam.transform.forward, toPredicted, strength);

        Vector3 delta = Quaternion.FromToRotation(cam.transform.forward, newForward).eulerAngles;

        Vector2 assistInput = new Vector2(-delta.y, delta.x);

        return rawInput + assistInput;
    }
}*/