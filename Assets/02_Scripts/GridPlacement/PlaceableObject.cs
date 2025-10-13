using Unity.Collections;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public Vector3Int Cell { get; private set; }
    public string Name { get; private set; }
    public ObjectData ObjectData {get; private set; }

    public bool Placed {get; private set;} = false;

    [ReadOnly]
    public PlaceableObjectSaveData PlaceableObjectData = new();  
    
    public virtual void Initialize(ObjectData objectData)
    {
        ObjectData = objectData;
        PlaceableObjectData.name = objectData.displayName;
        PlaceableObjectData.ID = SaveData.GenerateID();
    }

    public virtual void Initialize(ObjectData objectData, PlaceableObjectSaveData placeableObjectData)
    {
        ObjectData = objectData;
        PlaceableObjectData = placeableObjectData;
    }

    public virtual void Place()
    {
        Placed = true;
        GameManager.Instance.SaveManager.OnSave += Save;
    }

    protected virtual void Save()
    {
        PlaceableObjectData.position = transform.position;
        GameManager.Instance.SaveManager.saveData.AddData(PlaceableObjectData);
    }

    private void OnDisable()
    {
        GameManager.Instance.SaveManager.OnSave -= Save;
    }
}
