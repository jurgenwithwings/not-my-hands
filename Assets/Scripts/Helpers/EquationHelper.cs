using UnityEngine;

public static class Luck {
#region Luck Curve

    private static float luckoutsideInfluence = 1f;
    private static float luckCurveBonus = 0.7f;
    private static float luckCurveExp = 0.8f;

    public static float GetLuckCurve(float luck, float outsideInfluence, float bonus, float exp) {
        bool isNegativeLuck = false;

        luck -= 1;

        switch (luck) {
            case 0:
                return 1;
            case < 0:
                isNegativeLuck = true;
                luck *= -1f;
                break;
        }

        float result = 1 + (bonus * Mathf.Pow(luck * outsideInfluence, exp));
        return isNegativeLuck ? 1 / result : result;
    }

    public static float GetLuckCurve(float luck, float bonus, float exp) {
        return GetLuckCurve(luck, luckoutsideInfluence, bonus, exp);
    }

    public static float GetLuckCurve(float luck, float outsideInfluence) {
        return GetLuckCurve(luck, outsideInfluence, luckCurveBonus, luckCurveExp);
    }

    public static float GetLuckCurve(float luck) {
        return GetLuckCurve(luck, luckoutsideInfluence, luckCurveBonus, luckCurveExp);
    }

#endregion
}

public static class EquationHelper {

    public static float RollCritMult(float critChance, float critMultiplier, float luck) {
        float luckMultiplier = Luck.GetLuckCurve(luck);
        
        float adjustedCritChance = critChance * luckMultiplier;
        
        float roll = Random.Range(0f, 100f);

        float finalMultiplier = 0f;
        
        for (int i = 0; i < Mathf.Floor(adjustedCritChance); i++) {
            finalMultiplier += critMultiplier;
        }
        if (roll < (adjustedCritChance % 1) * 100f) {
            finalMultiplier += critMultiplier;
        }
        return finalMultiplier > 0 ? finalMultiplier : 1f;
    }
    
    public static int RollStatus(float statusChance, float luck, float procCoefficient = 1f) {
        float luckMultiplier = Luck.GetLuckCurve(luck);
        
        float adjustedStatusChance = statusChance * procCoefficient;
        adjustedStatusChance *= luckMultiplier;
        
        int stacks = (int)Mathf.Floor(adjustedStatusChance);
        
        float roll = Random.Range(0f, 100f);
        if (roll < (adjustedStatusChance % 1) * 100f) {
            stacks++;
        }

        return stacks;
    }
}
