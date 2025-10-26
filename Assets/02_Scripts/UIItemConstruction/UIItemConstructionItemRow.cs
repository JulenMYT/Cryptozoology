using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIItemConstructionItemRow : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Transform buttonsParent;
    [SerializeField] private UIItemConstructionSimpleButton itemButtonPrefab;

    [SerializeField] private float movementAmount = 105f;

    private Dictionary<string, UIItemConstructionSimpleButton> itemButtons = new();
    private static int buttonCountPerRow = 6;
    private int currentButtonCount = 0;
    private int currentButtonIndex = 0;
    public void Initialize(string itemName)
    {
        itemNameText.text = itemName;
    }

    public UIItemConstructionSimpleButton AddButton(ObjectData data)
    {
        UIItemConstructionSimpleButton newButton = Instantiate(itemButtonPrefab, buttonsParent);
        newButton.Initialize(data);
        itemButtons.Add(data.displayName, newButton);
        currentButtonCount++;
        return newButton;
    }

    public void Clear()
    {
        foreach (var button in itemButtons.Values)
        {
            Destroy(button.gameObject);
        }
        itemButtons.Clear();
        currentButtonCount = 0;
        currentButtonIndex = 0;
    }

    public bool CanGoLeft()
    {
        if (currentButtonCount <= buttonCountPerRow)
        {
            return false;
        }

        if (currentButtonIndex < 1)
        {
            return false;
        }

        return true;
    }

    public bool CanGoRight()
    {
        if (currentButtonCount <= buttonCountPerRow)
        {
            return false;
        }

        if (currentButtonIndex + buttonCountPerRow >= currentButtonCount)
        {
            return false;
        }

        return true;
    }

    public void GoLeft()
    {
        currentButtonIndex--;
        buttonsParent.localPosition += new Vector3(movementAmount, 0f, 0f);
    }

    public void GoRight()
    {
        currentButtonIndex++;
        buttonsParent.localPosition += new Vector3(-movementAmount, 0f, 0f);
    }
}
