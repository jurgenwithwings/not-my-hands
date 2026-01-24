using UnityEngine;

public enum LimbType {
    Arm = 1,
    Leg = 2,
}

public enum LimbSide {
    Left = 1,
    Right = 2,
}

public class LimbData : ItemData
{
    public LimbType limbType;
}
