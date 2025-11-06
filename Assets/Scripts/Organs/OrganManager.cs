using System;
using UnityEngine;

public class OrganManager : MonoBehaviour {
    private Statboard statboard;

    private Organ[] organs = { new Heart(), new Lungs(), new Brain(), new Liver() };
    
    private void Awake() {
        statboard = GetComponent<Statboard>();
        OrganHelper.InitialiseOrganArray(organs, statboard);
    }

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