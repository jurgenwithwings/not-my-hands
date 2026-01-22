using System;
using UnityEngine;

public class OrganManager : MonoBehaviour {
    private Statboard Statboard;

    private Organ[] organs = { new Heart(), new Brain(), new Liver() };

    public void SetStatboard(Statboard statboard) {
        if (Statboard != null) {
            Statboard = statboard;
        }
        Statboard = GetComponent<Statboard>();
        OrganHelper.InitialiseOrganArray(organs, Statboard);
    }

    public void AddOrgan(ClassReference<Organ> organ, OrganData data) {
        int index = (int)data.type;
        organs[index] = organ.CreateInstance();
        organs[index].Initialise(Statboard, data);
    }

    public void Update() {
        foreach (Organ organ in organs) {
            organ.Tick();
        }
    }

    public void RemoveOrgan(OrganType organType) {
        organs[(int)organType].Remove();
        organs[(int)organType] = OrganHelper.GetDefaultOrgan(organType, Statboard);
    }
}