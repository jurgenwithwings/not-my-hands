public class PlayerHealthBar : HealthBar {
    // Null Functionality
    protected override void Awake() { }
    protected override void Start() { }
    protected override void Update() { }
    protected override void LateUpdate() { }
    protected override bool IsLookedAt(float distance) => false;
    
    protected override void SetSliderValue() {
        PlayerHUDEvents.OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}