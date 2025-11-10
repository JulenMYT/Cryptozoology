using System;
using TMPro;
using UnityEngine;

public class UIMoney : MonoBehaviour
{
    [SerializeField]
    private TMP_Text moneyText;

    private void Start()
    {
        MoneyManager moneyManager = GameManager.Instance.Money;
        moneyManager.OnMoneyChanged += UpdateMoneyDisplay;
        UpdateMoneyDisplay(moneyManager.Money);
    }

    private void UpdateMoneyDisplay(int money)
    {
        moneyText.text = money.ToString();
    }
}
