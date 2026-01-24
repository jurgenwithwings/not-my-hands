using System;
using UnityEngine;

public class OrganManager : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        OrganHelper.InitialiseOrganArray(organs, statboard);
    }

    public Organ[] organs { get; private set; }= { new Heart(), new Brain(), new Liver() };

    public void AddOrgan(ClassReference<Organ> organ, OrganData data) {
        int index = (int)data.type;
        OrganData old = organs[index].data;
        organs[index] = organ.CreateInstance();
        organs[index].Initialise(statboard, data);
        statboard.eventManager?.OnOrganChanged?.Invoke(organs[index].data, old);
        Instantiate(old.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(transform.forward + (transform.up * 0.4f) * 2f);
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