using System;
using UnityEngine;

[Serializable]
public enum OrganType {
    Heart = 0,
    Lungs = 1,
    Brain = 2,
    Liver = 3,
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

    public virtual void Tick() {
        
    }

    public virtual void Remove() {
        
    }
}

public static class OrganHelper {
    private static string OrganDataPath = "DefaultOrgans/";
    private static string[] enumArray => Enum.GetNames(typeof(OrganType));
    
    private static OrganData LoadOrganData(string path) {
        return Resources.Load<OrganData>(OrganDataPath + path);
    }

    public static void InitialiseOrganArray(Organ[] array, Statboard statboard) {
        for (int i = 0; i < enumArray.Length; i++) {
            array[i].Initialise(statboard, LoadOrganData(enumArray[i]));
        }
    }

    public static Organ GetDefaultOrgan(OrganType organType, Statboard statboard) {
        Organ newOrgan = organType switch {
            OrganType.Heart => new Heart(),
            OrganType.Lungs => new Lungs(),
            OrganType.Brain => new Brain(),
            OrganType.Liver => new Liver(),
            _ => throw new ArgumentOutOfRangeException(nameof(organType), organType, null)
        };
        newOrgan.Initialise(statboard, LoadOrganData(organType.ToString()));
        return newOrgan;
    }
}

public class Heart : Organ { }
public class Lungs : Organ { }
public class Brain : Organ { }
public class Liver : Organ { }