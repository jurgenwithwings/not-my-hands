using UnityEngine;

public class TestArmProjectile : MonoBehaviour {
    [SerializeField] float lifetime = 5f;
    [SerializeField] Rigidbody rigidbody;

    private Statboard source;
    private Damage damage;
    private float statusChance;
    
    public void Initialise(Statboard source, Damage damage, float statusChance, float force) {
        this.source = source;
        this.damage = damage;
        this.statusChance = statusChance;
        
        rigidbody.AddForce(transform.forward * force, ForceMode.Impulse);
        
        InvokeRepeating(nameof(Timeout), lifetime, lifetime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject != source.gameObject && other.TryGetComponent(out Statboard victim)) {
            DamageInfo info = new DamageInfo(damage, source, transform.position) {
                sourceStatusChance = statusChance,
            };
            victim.health?.TakeDamage(info);
        }
        
        if (other.gameObject != source.gameObject) {
            Destroy(gameObject);
        }
    }

    private void Timeout() {
        Destroy(gameObject);
    }
}