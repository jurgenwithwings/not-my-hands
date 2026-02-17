using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour {
    [SerializeField] protected float lifetime = 5f;
    [SerializeField] protected Rigidbody rigidbody;
    [SerializeField] protected Collider collider;
    [SerializeField] protected bool destroyOnContact = true;
    [SerializeField] protected float hideOnDestroyDelay;
    [SerializeField] protected VisualEffect[] visualEffects;
    
    protected Statboard source;
    protected Damage damage;
    protected float statusChance;

    protected float elapsedTime;
    protected float T => elapsedTime / lifetime;
    
    public virtual void Initialise(Statboard source, Damage damage, float statusChance, Vector3 force) {
        this.source = source;
        this.damage = damage;
        this.statusChance = statusChance;
        
        rigidbody.AddForce(force, ForceMode.Impulse);
        
        Invoke(nameof(Remove), lifetime);
    }

    protected virtual void Update() {
        elapsedTime += Time.deltaTime;
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
        DamageInfo info = new(damage, source, transform.position) {
            sourceStatusChance = statusChance,
        };
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