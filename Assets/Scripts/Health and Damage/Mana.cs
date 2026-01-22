using UnityEngine;

public class Mana : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }

    public void StatboardFinishedSet() {
        CurrentMana = statboard.maxMana;
    }
    
    public float CurrentMana { get; private set; }

    public bool RemoveMana(float amount) {
        bool result = false;
        if (CurrentMana >= amount) {
            CurrentMana -= amount;
            statboard.eventManager.OnManaChanged?.Invoke(-amount);
            result = true;
        }
        return result;
    }

    public float AddMana(float amount) {
        float difference = 0;
        if (CurrentMana < statboard.maxMana) {
            difference = CurrentMana;
            CurrentMana += amount;
            CurrentMana = Mathf.Min(CurrentMana, statboard.maxMana);
            difference = CurrentMana - difference;
            statboard.eventManager.OnManaChanged?.Invoke(difference);
        }
        return difference;
    }
    
    private void Update() {
        if (CurrentMana < statboard.maxMana) {
            CurrentMana += statboard.manaRegenRate * Time.deltaTime;
            CurrentMana = Mathf.Min(CurrentMana, statboard.maxMana);
        }
    }
}