using System;
using UnityEngine;

[Serializable]
public enum OrganType {
    Heart = 0,
    Brain = 1,
    Liver = 2,
}

[Serializable]
public struct OrganStat {
    public Statboard.VariableType statType;
    public float value;
}

[Serializable]
public abstract class Organ {
    protected Statboard stats;

    public OrganData data;
    
    public virtual void Initialise(Statboard statboard, OrganData organData) {
        stats = statboard;
        data = organData;

        foreach (OrganStat stat in data.stats) {
            stats.GetStatByEnum(stat.statType).BaseValue = stat.value;
        }
    }

    public virtual void Tick() { }

    public virtual void Remove() { }
}

public static class OrganHelper {
    private static string OrganDataPath = "DefaultOrgans/";
    private static string[] enumArray => Enum.GetNames(typeof(OrganType));
    
    // Load OrganData from Resources folder
    private static OrganData LoadOrganData(string path) {
        return Resources.Load<OrganData>(OrganDataPath + path);
    }

    /// <summary>
    /// Fills an array of Organs with one of each default Organ type from the Resources folder, and initialises them to the given statboard.
    /// </summary>
    /// <param name="array">The array you would like to have the stored organs in.</param>
    /// <param name="statboard">The statboard that the organs will belong to.</param>
    public static void InitialiseOrganArray(Organ[] array, Statboard statboard) {
        for (int i = 0; i < enumArray.Length; i++) {
            array[i].Initialise(statboard, LoadOrganData(enumArray[i]));
        }
    }

    /// <summary>
    /// Returns an organ of the specified type, initialised to the given statboard, with default stats from the Resources folder.
    /// </summary>
    /// <param name="organType">The type of organ to be returned.</param>
    /// <param name="statboard">The statboard the organ will belong to.</param>
    /// <returns>The new initialised Organ that was created.</returns>
    public static Organ GetDefaultOrgan(OrganType organType, Statboard statboard) {
        Organ newOrgan = organType switch {
            OrganType.Heart => new Heart(),
            OrganType.Brain => new Brain(),
            OrganType.Liver => new Liver(),
            _ => throw new ArgumentOutOfRangeException(nameof(organType), organType, null)
        };
        newOrgan.Initialise(statboard, LoadOrganData(organType.ToString()));
        return newOrgan;
    }
}

//Empty Classes for the default organs
public class Heart : Organ { }
public class Brain : Organ { }
public class Liver : Organ { }


// TEMP TEMP TEMP TEMP
public class StatusEffectHeart : Organ {
    public float multiplier = 0.5f;
    
    public override void Initialise(Statboard statboard, OrganData organData) {
        base.Initialise(statboard, organData);

        stats.eventManager.OnPreSendDamage += ModifyDamage;
    }


    private void ModifyDamage(DamageInfo damageInfo, Statboard victim, Statboard self) {
        for (int i = 0; i < damageInfo.damageInstances.Length; i++) {
            damageInfo.damageInstances[i].amount *=
                1 + (multiplier * victim.statusEffectManager.BuffEffects.Count);
        }
    }
}