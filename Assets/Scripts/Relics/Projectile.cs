using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour {
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected Rigidbody rigidbody;
    [SerializeField] protected Collider collider;
    [SerializeField] protected bool destroyOnContact = true;
    [SerializeField] protected float hideOnDestroyDelay;
    [SerializeField] protected VisualEffect[] visualEffects;
    [SerializeField] protected float speed;

    private float calculatedSpeed => speed * source.projectileSpeedMultiplier.Value;
    
    protected Statboard source;
    protected Damage damage;

    public List<object> tags = new();

    protected float elapsedTime;
    protected float T => elapsedTime / lifetime;
    
    public virtual void Initialise(Statboard source, Damage damage, float speed, List<object> tags = null) {
        this.source = source;
        this.damage = damage;
        this.speed = speed;
        if (tags != null) {
            this.tags.AddRange(tags);
        }
        
        Invoke(nameof(Remove), lifetime);
    }

    protected virtual void Update() {
        elapsedTime += Time.deltaTime;
        transform.position += transform.forward * calculatedSpeed * Time.deltaTime;
    }
    
    private void OnTriggerEnter(Collider other) {
        if (other.gameObject != source.gameObject && other.TryGetComponent(out Statboard victim)) {
            HitTarget(other, victim);
        }
        
        if (destroyOnContact && other.gameObject != source.gameObject) {
            Remove();
        }
    }

    protected virtual void HitTarget(Collider other, Statboard victim) {
        DamageInfo info = new(damage, source, transform.position);
        if (tags.Count > 0) {
            info.tags.AddRange(tags);
        }
        victim.health?.TakeDamage(info);
    }
    
    protected virtual void Remove() {
        Invoke(nameof(Destroy), hideOnDestroyDelay);

        if (visualEffects.Length > 0) {
            foreach (VisualEffect vfx in visualEffects) {
                vfx.Stop();
            }
        }
        
        collider.enabled = false;
    }

    protected void Destroy() {
        Destroy(gameObject);
    }
}