using System;
using System.Collections.Generic;
using UnityEngine;

public enum BuildingMode
{
    None,
    Placement,
    Removal
}

public class BuildingSystem : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private Grid grid;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private LayerMask gridLayer;

    [Header("Placement Objects")]
    [SerializeField] private UICursor cursor;
    private Transform objectsParent;

    private ObjectData selectedItem;
    private GameObject previewObject;
    private readonly ObjectGrid objectGrid = new();
    private BuildingMode currentMode = BuildingMode.None;

    public event Action OnCanceled;
    public event Action<BuildingMode> OnModeChanged;

    public bool IsActive => currentMode != BuildingMode.None;
    public BuildingMode CurrentMode => currentMode;

    private void Awake()
    {
        if (!objectsParent)
        {
            var parentGO = new GameObject("ObjectsParent");
            objectsParent = parentGO.transform;
        }

        if (cursor) cursor.SetActive(false);
    }

    private void Update()
    {
        if (!IsActive || !RaycastGrid(out RaycastHit hit)) return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (!IsInsideGrid(cellPos, selectedItem?.gridSize ?? Vector2Int.one)) return;

        switch (currentMode)
        {
            case BuildingMode.Placement:
                Vector3 previewPos = hit.point;
                MovePreview(previewPos);
                cursor.UpdateColor(CanPlaceObject(cellPos, selectedItem.gridSize));
                break;

            case BuildingMode.Removal:
                cursor.UpdateColor(objectGrid.IsOccupied(cellPos));
                break;
        }

        UpdateCursor(cellPos);
    }

    public void SetPlacementMode(ObjectData item)
    {
        if (item == null) return;

        EnterMode(BuildingMode.Placement);
        selectedItem = item;

        ResetPreview();
        CreatePreview(item.prefab);
        SetupCursor(item.gridSize);
    }

    public void SetRemoveMode()
    {
        EnterMode(BuildingMode.Removal);

        ResetPreview();
        SetupCursor(Vector2Int.one);
    }

    private void EnterMode(BuildingMode mode)
    {
        currentMode = mode;
        cursor?.SetActive(true);
        OnModeChanged?.Invoke(mode);
        GameManager.Instance?.GameModeManager?.SetMode(GameMode.Building);
    }

    private void ExitMode()
    {
        currentMode = BuildingMode.None;
        selectedItem = null;

        ResetPreview();
        cursor?.SetActive(false);

        OnCanceled?.Invoke();

        GameManager.Instance?.GameModeManager?.SetMode(GameMode.Normal);
    }

    private bool TryGetValidCell(out Vector3Int cell)
    {
        cell = default;
        if (!RaycastGrid(out RaycastHit hit)) return false;
        cell = grid.WorldToCell(hit.point);
        if (!IsInsideGrid(cell, selectedItem?.gridSize ?? Vector2Int.one)) return false;
        return true;
    }

    public void TryPlaceAtCursor()
    {
        if (!selectedItem || !TryGetValidCell(out Vector3Int cell)) return;

        if (selectedItem.cost > GameManager.Instance.Money.Money) return;
        if (!IsInsideGrid(cell, selectedItem.gridSize)) return;
        if (!CanPlaceObject(cell, selectedItem.gridSize)) return;

        Vector3 spawnPos = grid.GetCellCenterWorld(cell);
        GameObject obj = PlaceItem(selectedItem, spawnPos);
        if (!obj) return;
        GameManager.Instance.Money.SpendMoney(selectedItem.cost);

        if (obj.TryGetComponent<PlaceableObject>(out var placeable))
        {
            placeable.Initialize(selectedItem);
            placeable.Place();
        }

        if (obj.TryGetComponent<Animal>(out var animal))
            animal.Initialize(AnimalType.Resident);

        if (selectedItem.isUnique)
            CancelPlacement();
    }

    public void TryRemoveAtCursor()
    {
        if (!TryGetValidCell(out Vector3Int cell)) return;

        if (objectGrid.TryGetObject(cell, out var obj))
            RemoveObject(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        if (!obj) return;

        if (obj.TryGetComponent<PlaceableObject>(out var placeable))
        {
            var data = placeable.ObjectData;
            Vector3Int cellPos = grid.WorldToCell(obj.transform.position);
            objectGrid.Remove(cellPos, data.gridSize);
        }

        Destroy(obj);
    }

    public GameObject PlaceItem(ObjectData data, Vector3 position)
    {
        if (data.isGridItem)
        {
            Vector3Int cellPos = grid.WorldToCell(position);
            Vector3 spawnPos = grid.GetCellCenterWorld(cellPos);

            if (!IsInsideGrid(cellPos, data.gridSize) || !CanPlaceObject(cellPos, data.gridSize))
                return null;

            var obj = Instantiate(data.prefab, spawnPos, Quaternion.identity, objectsParent);
            objectGrid.Register(cellPos, obj, data.gridSize);
            return obj;
        }

        return Instantiate(data.prefab, position, Quaternion.identity, objectsParent);
    }

    private void MovePreview(Vector3 position)
    {
        if (previewObject)
            previewObject.transform.position = position;
    }

    private void UpdateCursor(Vector3Int cellPos)
    {
        if (cursor)
            cursor.UpdatePosition(grid.CellToWorld(cellPos));
    }

    public void CancelPlacement()
    {
        ExitMode();
    }

    private void CreatePreview(GameObject prefab)
    {
        if (prefab)
        {
            previewObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            foreach (var col in previewObject.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }
    }

    private void ResetPreview()
    {
        if (previewObject)
        {
            Destroy(previewObject);
            previewObject = null;
        }
    }

    private void SetupCursor(Vector2Int size)
    {
        if (cursor)
        {
            cursor.SetSize(size);
            cursor.UpdateColor(true);
        }
    }

    private bool RaycastGrid(out RaycastHit hit)
    {
        return Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f, gridLayer);
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
    private readonly Dictionary<Vector3Int, GameObject> grid = new();

    public void Register(Vector3Int cell, GameObject obj, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                grid[new Vector3Int(cell.x + x, cell.y, cell.z + z)] = obj;
    }

    public void Remove(Vector3Int cell, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                grid.Remove(new Vector3Int(cell.x + x, cell.y, cell.z + z));
    }

    public bool IsOccupied(Vector3Int cell) => grid.ContainsKey(cell);
    public bool TryGetObject(Vector3Int cell, out GameObject obj) => grid.TryGetValue(cell, out obj);
}
