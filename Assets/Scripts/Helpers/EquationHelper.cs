using Stats;
using UnityEngine;

public static class Luck {
#region Luck Curve

    private static float luckoutsideInfluence = 1f;
    private static float luckCurveBonus = 0.3f;
    private static float luckCurveExp = 0.7f;

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

    public static float RollCritMult(ref DamageInfo info) {
        float luckMultiplier = Luck.GetLuckCurve(info.source.luck.Value);
        
        float adjustedCritChance = info.sourceCriticalChance * info.source.criticalChanceMultiplier.Value * luckMultiplier;
        Debug.Log(adjustedCritChance);

        info.resultingCritLevel = Mathf.FloorToInt(adjustedCritChance);
        
        float roll = Random.value;
        if (roll < adjustedCritChance % 1) {
            info.resultingCritLevel++;
        }

        float finalMultiplier = Mathf.Pow(info.source.criticalDamageMultiplier.Value, info.resultingCritLevel);
        info.AddModifier(finalMultiplier, ModifierType.FinalMultiplicative, "RollCrit");
        
        return finalMultiplier;
    }
    
    //TODO: Implement into status manager
    public static int RollStatus(float entityStatusChance, float luck, float procCoefficient = 1f) {
        float luckMultiplier = Luck.GetLuckCurve(luck);
        
        float adjustedStatusChance = entityStatusChance * procCoefficient;
        adjustedStatusChance *= luckMultiplier;
        
        int stacks = (int)Mathf.Floor(adjustedStatusChance);
        
        float roll = Random.Range(0f, 100f);
        if (roll < (adjustedStatusChance % 1) * 100f) {
            stacks++;
        }

        return stacks;
    }
}
