using System;
using UnityEngine;

public class OrganManager : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        OrganHelper.InitialiseOrganArray(organs, statboard);
    }

    private Organ[] organs = { new Heart(), new Brain(), new Liver() };

    public void AddOrgan(ClassReference<Organ> organ, OrganData data) {
        int index = (int)data.type;
        organs[index] = organ.CreateInstance();
        organs[index].Initialise(statboard, data);
    }

    public void Update() {
        foreach (Organ organ in organs) {
            organ.Tick();
        }
    }

    public void RemoveOrgan(OrganType organType) {
        organs[(int)organType].Remove();
        organs[(int)organType] = OrganHelper.GetDefaultOrgan(organType, statboard);
    }
}