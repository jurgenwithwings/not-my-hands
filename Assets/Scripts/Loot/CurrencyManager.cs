using UnityEngine;

public class CurrencyManager : MonoBehaviour {
    private Statboard statboard;
    
    [SerializeField] private int startingCurrency = 100;
    public int CurrencyAmount { get; set; }

    private void Awake() {
        CurrencyAmount = startingCurrency;
        statboard = GetComponent<Statboard>();
    }

    public int AddCurrency(int amount) {
        int scaledAmount = Mathf.RoundToInt(amount * statboard.currencyMultiplier.Value);
        
        CurrencyAmount += scaledAmount;
        PlayerHUDEvents.OnMoneyChanged?.Invoke(CurrencyAmount);
        return scaledAmount;
    }

    public bool RemoveCurrency(int amount) {
        if (amount < CurrencyAmount) {
            CurrencyAmount -= amount;
            PlayerHUDEvents.OnMoneyChanged?.Invoke(CurrencyAmount);
            return true;
        }
        return false;
    }

    public bool HasAmount(int amount) {
        if (amount < CurrencyAmount) {
            return true;
        }
        return false;
    }
}