using UnityEngine;
using UnityEngine.UI;

public class HUDHealthBar : MonoBehaviour{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Slider secondaryHealthBar;

    private float targetHealth = 1;

    private float lastDamageTime;
    private float secondaryHealthMoveDelay = 1.68f;
    
    void Start() {
        PlayerHUDEvents.OnSetHealth += SetTargetHealth;
        healthBar.value = 1;
        secondaryHealthBar.value = 1;
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnSetHealth -= SetTargetHealth;
    }

    private void SetTargetHealth(float percent) {
        if (percent < targetHealth && 
                 Mathf.Approximately(secondaryHealthBar.value, healthBar.value)) { // If health lost, and anim not in progress. Start Timer.
            lastDamageTime = Time.time;
        }
        targetHealth = percent;
        healthBar.value = percent;
        
        if (percent > targetHealth || secondaryHealthBar.value - percent <= 0.01f) { //If gaining health or small damage, then skip animation.
            secondaryHealthBar.value = targetHealth;
        }
    }

    private void Update() {
        if (Time.time - lastDamageTime > secondaryHealthMoveDelay) {
            secondaryHealthBar.value = Mathf.MoveTowards(secondaryHealthBar.value, targetHealth, Time.deltaTime * 0.5f);
        }
    }
}