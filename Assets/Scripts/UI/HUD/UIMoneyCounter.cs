using System;
using TMPro;
using UnityEngine;

public class UIMoneyCounter : MonoBehaviour {
    [SerializeField] private TMP_Text moneyText;

    private void Start() {
        PlayerHUDEvents.OnMoneyChanged += OnMoneyChanged;
    }

    private void OnMoneyChanged(int money) {
        moneyText.text = $"${money}";
    }

    private void OnDestroy() {
        PlayerHUDEvents.OnMoneyChanged -= OnMoneyChanged;
    }
}