using System;
using UnityEngine;

public static class LayerExtentions {
    public static bool IsInLayerMask(this GameObject obj, LayerMask mask) {
        return (mask.value & (1 << obj.layer)) != 0;
    }

    public static bool IsInLayerMask(int layer, LayerMask mask) {
        return (mask.value & (1 << layer)) != 0;
    }

    public static LayerMask AddLayerToMask(this LayerMask mask, int layer) {
        return (mask.value | (1 << layer));
    }

    public static LayerMask RemoveLayerFromMask(this LayerMask mask, int layer) {
        return (mask.value & ~(1 << layer));
    }
}
