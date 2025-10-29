using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField]
    private int startingMoney = 1000;

    public int Money { get; private set; }

    private void Awake()
    {
        Money = startingMoney;
    }

    private void Start()
    {
        SaveManager.Instance.OnSave += Save;
    }

    public void AddMoney(int amount)
    {
        Money += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (amount > Money)
            return false;
        Money -= amount;
        return true;
    }

    private void Save()
    {
        MoneySaveData data = new MoneySaveData
        {
            currentMoney = Money
        };
    }

    public void Load(MoneySaveData data)
    {
        if (data == null)
            return;
        Money = data.currentMoney;
    }
}
