using System;
using UnityEngine;

public class OrganManager : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }
    public void StatboardFinishedSet() {
        OrganHelper.InitialiseOrganArray(organs, statboard);
    }

    public Organ[] organs { get; private set; }= { new Heart(), new Brain(), new Liver() };

    public void AddOrgan(OrganData organ) {
        int index = (int)organ.type;
        
        OrganData old = organs[index].data;
        
        organs[index] = Activator.CreateInstance(organ.Type()) as Organ;
        organs[index].Initialise(statboard, organ);
        
        statboard.eventManager?.OnOrganChanged?.Invoke(organs[index].data, old);
        
        Instantiate(old.prefab, transform.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(transform.forward + (transform.up * 0.4f) * 2f);
    }

    public void Update() {
        foreach (Organ organ in organs) {
            organ.Update();
        }
    }

    public bool RemoveOrgan(OrganType organType) {
        OrganData old = organs[(int)organType].data;
        organs[(int)organType].Remove();
        organs[(int)organType] = OrganHelper.GetDefaultOrgan(organType, statboard);
        statboard.eventManager?.OnOrganChanged?.Invoke(organs[(int)organType].data, old);
        return true;
    }
}