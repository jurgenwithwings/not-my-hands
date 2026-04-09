using System;
using UnityEngine;

public class EntityRespawn : MonoBehaviour {
    private GameObject prefab;

    private void Start() {
        prefab = Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
        if (prefab.TryGetComponent(out EntityRespawn er)) {
            er.enabled = false;
        }
        prefab.SetActive(false);
        
        if (gameObject.TryGetComponent(out Health health)) {
            health.OnDeath += Respawn;
        }
        else if (gameObject.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp += Respawn;
        }
    }

    private void OnDestroy() {
        if (gameObject.TryGetComponent(out Health health)) {
            health.OnDeath -= Respawn;
        }
        else if (gameObject.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp -= Respawn;
        }
    }

    private void Respawn(Statboard _) {
        Respawn();
    }

    private void Respawn() {
        if (gameObject.TryGetComponent(out Health health)) {
            health.OnDeath -= Respawn;
        }
        else if (gameObject.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp -= Respawn;
        }
        
        prefab.SetActive(true);
        if (prefab.TryGetComponent(out EntityRespawn er)) {
            er.enabled = true;
        }
    }
}