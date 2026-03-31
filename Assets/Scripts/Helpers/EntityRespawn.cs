using System;
using UnityEngine;

public class EntityRespawn : MonoBehaviour {
    private GameObject entityPrefab;
    private GameObject activeEntity;

    private void Start() {
        entityPrefab = gameObject;
        
        activeEntity = entityPrefab;
        entityPrefab = Instantiate(activeEntity, activeEntity.transform.position, activeEntity.transform.rotation);
        entityPrefab.SetActive(false);

        if (activeEntity.TryGetComponent(out Health health)) {
            health.OnDeath += Respawn;
        }
        else if (activeEntity.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp += Respawn;
        }
    }

    private void OnDestroy() {
        if (activeEntity == null) return;
        
        if (activeEntity.TryGetComponent(out Health health)) {
            health.OnDeath -= Respawn;
        }
        else if (activeEntity.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp -= Respawn;
        }
    }

    private void Respawn(Statboard _) {
        Respawn();
    }

    private void Respawn() {
        if (activeEntity.TryGetComponent(out Health health)) {
            health.OnDeath -= Respawn;
        }
        else if (activeEntity.TryGetComponent(out PhysicalLoot physicalLoot)) {
            physicalLoot.OnPickUp -= Respawn;
        }
        
        activeEntity = Instantiate(entityPrefab, entityPrefab.transform.position, entityPrefab.transform.rotation);
        activeEntity.SetActive(true);
        
        if (activeEntity.TryGetComponent(out Health health2)) {
            health2.OnDeath += Respawn;
        }
        else if (activeEntity.TryGetComponent(out PhysicalLoot physicalLoot2)) {
            physicalLoot2.OnPickUp += Respawn;
        }
    }
}