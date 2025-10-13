using System.Collections.Generic;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private LayerMask gridLayer;

    [Header("Placement Objects")]
    [SerializeField] private Transform objectsParent;
    [SerializeField] private PlacementCursor cursor;

    private ObjectData selectedItem;
    private GameObject previewObject;
    private ObjectGrid objectGrid = new ObjectGrid();
    private bool removeMode = false;

    private void OnDisable()
    {
        UnsubscribeClicks();
    }

    private void Update()
    {
        if (selectedItem == null && !removeMode) return;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer)) return;


        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (!IsInsideGrid(cellPos, selectedItem?.gridSize ?? Vector2Int.one)) return;

        if (!removeMode) MovePreview(hit.point);
        UpdateCursor(cellPos);  
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
        GameManager.Instance.GameModeManager.SetMode(GameMode.Placement);
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

        GameManager.Instance.GameModeManager.SetMode(GameMode.Placement);
    }

    private void SubscribeClicks()
    {
        UnsubscribeClicks();
        GameManager.Instance.Input.OnLeftClick += HandleLeftClick;
        GameManager.Instance.Input.OnRightClick += HandleRightClick;
    }

    private void UnsubscribeClicks()
    {
        GameManager.Instance.Input.OnLeftClick -= HandleLeftClick;
        GameManager.Instance.Input.OnRightClick -= HandleRightClick;
    }

    private void HandleLeftClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;

        if (removeMode) TryRemoveAtCursor();
        else TryPlaceAtCursor();
    }

    private void HandleRightClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;
        CancelPlacement();
    }

    private void TryPlaceAtCursor()
    {
        if (selectedItem == null) return;
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer)) return;

        GameObject obj = PlaceItem(selectedItem, hit.point);
        if (!obj) return;

        obj.TryGetComponent<PlaceableObject>(out var placeableObject);
        if (placeableObject != null)
        {
            placeableObject.Initialize(selectedItem);
            placeableObject.Place();
        }

        if (selectedItem.isUnique)
            CancelPlacement();
    }

    private void TryRemoveAtCursor()
    {
        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer)) return;

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

        if (obj.TryGetComponent<PlaceableObject>(out var placeableObject))
        {
            objectGrid.Remove(obj);
            GameManager.Instance.Garden.RemoveObject(placeableObject.Name, obj);
        }

        Destroy(obj);
    }

    public GameObject PlaceItem(ObjectData data, Vector3 position)
    {
        Vector3Int cellPos = grid.WorldToCell(position);
        position = grid.GetCellCenterWorld(cellPos);

        if (!IsInsideGrid(cellPos, data.gridSize)) return null;
        if (!CanPlaceObject(cellPos, data.gridSize)) return null;

        GameObject obj = Instantiate(data.prefab, position, Quaternion.identity, objectsParent);

        if (data.category == ItemCategory.Animal)
        {
            if (obj.TryGetComponent<Animal>(out var animal))
                animal.Place();
        }
        else
        {
            objectGrid.Register(cellPos, obj, data.gridSize);
        }

        GameManager.Instance.Garden.AddObject(data.displayName, obj);

        return obj;
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

    private void CancelPlacement()
    {
        DestroyPreview();
        selectedItem = null;
        removeMode = false;
        UnsubscribeClicks();

        if (cursor != null)
            cursor.SetActive(false);

        GameManager.Instance.GameModeManager.SetMode(GameMode.Normal);
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
        if (obj.TryGetComponent<PlaceableObject>(out var placeableObject))
        {
            Vector2Int size = placeableObject.ObjectData.gridSize;
            for (int x = 0; x < size.x; x++)
                for (int z = 0; z < size.y; z++)
                    grid.Remove(new Vector3Int(placeableObject.Cell.x + x, placeableObject.Cell.y, placeableObject.Cell.z + z));
        }
    }

    public bool IsOccupied(Vector3Int cell) => grid.ContainsKey(cell);
    public bool TryGetObject(Vector3Int cell, out GameObject obj) => grid.TryGetValue(cell, out obj);
}
