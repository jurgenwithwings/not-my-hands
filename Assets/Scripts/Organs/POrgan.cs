using System;
using NUnit.Framework.Internal;
using UnityEngine;

public class POrgan : PhysicalLoot, IInteractable {
    [SerializeField] private OrganData data;
    public string InteractionName() => data.displayName;
    public bool HasAltInteraction { get; }
    public void Interact(Statboard interactor) {
        OrganManager organManager = interactor.GetComponent<OrganManager>();
        if (organManager != null) {
            organManager.AddOrgan(data.organClass, data);
            Destroy(gameObject);
        }
    }
}