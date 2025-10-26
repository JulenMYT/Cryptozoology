using Unity.Collections;
using UnityEngine;

public class PlaceableObject : MonoBehaviour
{
    public ObjectData ObjectData {get; protected set; }

    public bool Placed {get; private set;} = false;

    [ReadOnly]
    protected PlaceableObjectSaveData PlaceableObjectData = new();  
    
    public virtual void Initialize(ObjectData objectData)
    {
        ObjectData = objectData;
        PlaceableObjectData.name = objectData.displayName;
        PlaceableObjectData.ID = SaveData.GenerateID();
        GameManager.Instance.Garden.AddObject(objectData.displayName, gameObject);
    }

    public virtual void Initialize(ObjectData objectData, PlaceableObjectSaveData placeableObjectData)
    {
        ObjectData = objectData;
        PlaceableObjectData = placeableObjectData;
        GameManager.Instance.Garden.AddObject(objectData.displayName, gameObject);
    }

    public virtual void Place()
    {
        Placed = true;
        SaveManager.Instance.OnSave += Save;
    }

    protected virtual void Save()
    {
        PlaceableObjectData.position = transform.position;
        SaveManager.Instance.saveData.AddData(PlaceableObjectData);
    }

    private void OnDisable()
    {
        SaveManager.Instance.OnSave -= Save;
    }

    protected virtual void OnDestroy()
    {
        if (Placed)
            GameManager.Instance.Garden.RemoveObject(ObjectData.displayName, gameObject);
    }
}
