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

    [HideInInspector] public OrganData data;
    
    public virtual void Initialise(Statboard statboard, OrganData organData) {
        stats = statboard;
        data = organData;

        foreach (OrganStat stat in data.stats) {
            stats.GetStatByEnum(stat.statType).SetBaseValue(stat.value);
        }
    }

    public virtual void Update() { }

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
[Serializable] public class Heart : Organ { }
[Serializable] public class Brain : Organ { }
[Serializable] public class Liver : Organ { }