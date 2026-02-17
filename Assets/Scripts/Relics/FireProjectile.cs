public class FireProjectile : Projectile {
    protected override void Update() {
        base.Update();
        
        if (visualEffects.Length > 0) {
            visualEffects[0].SetFloat("SizeMult", T + 0.2f);
            visualEffects[0].SetFloat("SpawnRadius", T * 0.5f);
        }
    }
}