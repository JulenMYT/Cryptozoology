using System.Collections.Generic;
using UnityEngine;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Transform objectsParent;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private PlacementCursor cursor;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private ItemUIManager itemUIManager;

    private ObjectData selectedItem;
    private GameObject previewObject;
    private Vector3Int previousCell;
    private ObjectGrid objectGrid = new ObjectGrid();
    private bool removeMode = false;
    private bool isSubscribedToClick = false;

    private void Start()
    {
        itemUIManager.OnItemSelected += SelectItem;
        itemUIManager.OnRemoveModeSelected += SetRemoveMode;
    }

    private void OnDisable()
    {
        itemUIManager.OnItemSelected -= SelectItem;
        itemUIManager.OnRemoveModeSelected -= SetRemoveMode;
        UnsubscribeClicks();
    }

    public void SelectItem(ObjectData item)
    {
        selectedItem = item;
        removeMode = false;
        DestroyPreview();

        if (item.prefab != null)
            previewObject = Instantiate(item.prefab, Vector3.zero, Quaternion.identity, objectsParent);

        if (cursor != null)
        {
            cursor.SetActive(true);
            cursor.SetSize(item.gridSize);
        }

        SubscribeClicks();
    }

    public void SetRemoveMode()
    {
        removeMode = true;
        DestroyPreview();
        SubscribeClicks();

        if (cursor != null)
        {
            cursor.SetActive(true);
            cursor.SetSize(Vector2Int.one);
            cursor.UpdateColor(false);
        }
    }

    private void Update()
    {
        if (selectedItem == null && !removeMode) return;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer))
            return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (!IsInsideGrid(cellPos, selectedItem?.gridSize ?? Vector2Int.one)) return;

        Vector3 placePos = grid.GetCellCenterWorld(cellPos);

        if (cellPos != previousCell)
        {
            previousCell = cellPos;
            if (!removeMode)
                MovePreview(placePos);
            UpdateCursor(cellPos);
        }
    }

    private void SubscribeClicks()
    {
        if (!isSubscribedToClick)
        {
            InputManager.Instance.OnLeftClick += HandleLeftClick;
            InputManager.Instance.OnRightClick += HandleRightClick;
            isSubscribedToClick = true;
        }
    }

    private void UnsubscribeClicks()
    {
        if (isSubscribedToClick)
        {
            InputManager.Instance.OnLeftClick -= HandleLeftClick;
            InputManager.Instance.OnRightClick -= HandleRightClick;
            isSubscribedToClick = false;
        }
    }

    private void HandleLeftClick()
    {
        if (InputManager.Instance.IsPointedOverUI()) return;

        if (removeMode)
            TryRemoveAtCursor();
        else
            TryPlaceAtCursor();
    }

    private void HandleRightClick()
    {
        if (InputManager.Instance.IsPointedOverUI()) return;
        if (!removeMode)
            CancelPlacement();
    }

    private void TryPlaceAtCursor()
    {
        if (selectedItem == null) return;

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer))
            return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        Vector3 placePos = grid.GetCellCenterWorld(cellPos);

        if (!IsInsideGrid(cellPos, selectedItem.gridSize)) return;

        PlaceItem(cellPos, placePos);
    }

    private void TryRemoveAtCursor()
    {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer))
            return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (objectGrid.TryGetObject(cellPos, out var obj))
            RemoveObject(obj);
    }

    public void RemoveObjectRequested(GameObject obj)
    {
        RemoveObject(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        if (obj == null) return;

        if (obj.TryGetComponent<ObjectIdentity>(out var identity))
        {
            objectGrid.Remove(obj);
            ObjectEvents.OnObjectRemoved?.Invoke(identity.Id, obj);
        }

        Destroy(obj);
    }

    private void MovePreview(Vector3 position)
    {
        if (previewObject != null)
            previewObject.transform.position = position;
    }

    private void UpdateCursor(Vector3Int cellPos)
    {
        if (cursor == null) return;

        Vector3 position = grid.CellToWorld(cellPos);
        cursor.UpdatePosition(position);
        if (!removeMode)
            cursor.UpdateColor(CanPlaceObject(cellPos, selectedItem?.gridSize ?? Vector2Int.one));
    }

    private void PlaceItem(Vector3Int cellPos, Vector3 position)
    {
        if (!CanPlaceObject(cellPos, selectedItem.gridSize)) return;

        GameObject obj = Instantiate(selectedItem.prefab, position, Quaternion.identity, objectsParent);

        if (!obj.TryGetComponent<ObjectIdentity>(out var identity))
            identity = obj.AddComponent<ObjectIdentity>();

        identity.Id = selectedItem.id;
        identity.Cell = cellPos;
        identity.Size = selectedItem.gridSize;

        switch (selectedItem.category)
        {
            case ItemCategory.Plant:
                if (obj.TryGetComponent<PlantBehaviour>(out var plant) && selectedItem is PlantDataSO plantData)
                    plant.Initialize(plantData);
                objectGrid.Register(cellPos, obj, selectedItem.gridSize);
                break;

            case ItemCategory.Animal:
                if (obj.TryGetComponent<IAnimal>(out var animal))
                    animal.SpawnAsVisitor();
                break;

            case ItemCategory.Production:
            case ItemCategory.Building:
            case ItemCategory.Resource:
                objectGrid.Register(cellPos, obj, selectedItem.gridSize);
                break;
            default:
                objectGrid.Register(cellPos, obj, selectedItem.gridSize);
                break;
        }

        ObjectEvents.OnObjectAdded?.Invoke(selectedItem.id, obj);
    }

    private void CancelPlacement()
    {
        DestroyPreview();
        selectedItem = null;
        removeMode = false;
        UnsubscribeClicks();

        if (cursor != null)
            cursor.SetActive(false);
    }

    private void DestroyPreview()
    {
        if (previewObject != null)
            Destroy(previewObject);
    }

    private bool CanPlaceObject(Vector3Int cellPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                if (objectGrid.IsOccupied(new Vector3Int(cellPos.x + x, cellPos.y, cellPos.z + z)))
                    return false;

        return true;
    }

    private bool IsInsideGrid(Vector3Int cellPos, Vector2Int size)
    {
        return cellPos.x >= 0 && cellPos.z >= 0 &&
               cellPos.x + size.x <= gridWidth &&
               cellPos.z + size.y <= gridHeight;
    }
}

public class ObjectGrid
{
    private Dictionary<Vector3Int, GameObject> grid = new();

    public void Register(Vector3Int cell, GameObject obj, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                grid[new Vector3Int(cell.x + x, cell.y, cell.z + z)] = obj;
    }

    public void Remove(GameObject obj)
    {
        if (obj.TryGetComponent<ObjectIdentity>(out var identity))
        {
            for (int x = 0; x < identity.Size.x; x++)
                for (int z = 0; z < identity.Size.y; z++)
                    grid.Remove(new Vector3Int(identity.Cell.x + x, identity.Cell.y, identity.Cell.z + z));
        }
    }

    public bool IsOccupied(Vector3Int cell) => grid.ContainsKey(cell);

    public bool TryGetObject(Vector3Int cell, out GameObject obj) => grid.TryGetValue(cell, out obj);
}
