using UnityEngine;

public class Mana : MonoBehaviour, IStatboard {
    public Statboard statboard { get; set; }

    public void StatboardFinishedSet() {
        CurrentMana = statboard.maxMana;
    }

    [SerializeField] private float regenDelay = 2f;
    private float regenTimer;
    
    public float CurrentMana { get; private set; }

    /// <summary>
    /// Removes the given amount of mana if the entity has enough.
    /// </summary>
    /// <param name="baseAmount">The base amount of mana wanting to be removed.</param>
    /// <returns>True if the entity has enough mana and mana was removed. False if the entity did not have enough mana.</returns>
    public bool RemoveMana(float baseAmount) {
        bool result = false;
        if (HasEnoughMana(baseAmount)) {
            CurrentMana -= baseAmount;
            statboard.eventManager.OnManaChanged?.Invoke(CurrentMana, statboard.maxMana);
            regenTimer = baseAmount <= 0 ? regenTimer : regenDelay;
            result = true;
        }
        return result;
    }

    public float AddMana(float baseAmount) {
        float difference = 0;
        if (CurrentMana < statboard.maxMana) {
            difference = CurrentMana;
            CurrentMana += baseAmount;
            CurrentMana = Mathf.Min(CurrentMana, statboard.maxMana);
            statboard.eventManager.OnManaChanged?.Invoke(CurrentMana, statboard.maxMana);
            regenTimer = regenDelay;
        }
        return difference;
    }
    
    private void Update() {
        if (CurrentMana < statboard.maxMana && regenTimer <= 0) {
            CurrentMana += statboard.manaRegenRate * Time.deltaTime;
            CurrentMana = Mathf.Min(CurrentMana, statboard.maxMana);
            statboard.eventManager.OnManaChanged?.Invoke(CurrentMana, statboard.maxMana);
        }
        regenTimer -= Time.deltaTime;
    }

    public bool HasEnoughMana(float baseAmount) {
        return CurrentMana >= baseAmount;
    }
}