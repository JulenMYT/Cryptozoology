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
    private Transform objectsParent;
    [SerializeField] private PlacementCursor cursor;

    private ObjectData selectedItem;
    private GameObject previewObject;
    private ObjectGrid objectGrid = new ObjectGrid();
    private BuildingMode currentMode = BuildingMode.None;

    private void Awake()
    {
        if (!objectsParent)
        {
            GameObject parentGO = new GameObject("ObjectsParent");
            objectsParent = parentGO.transform;
        }
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (currentMode == BuildingMode.None || !RaycastGrid(out RaycastHit hit)) return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (!IsInsideGrid(cellPos, selectedItem?.gridSize ?? Vector2Int.one)) return;

        if (currentMode == BuildingMode.Placement) MovePreview(hit.point);
        UpdateCursor(cellPos);
    }

    public void SetPlacementMode(ObjectData item)
    {
        gameObject.SetActive(true);

        selectedItem = item;
        currentMode = BuildingMode.Placement;

        ResetPreview();
        CreatePreview(item.prefab);
        SetupCursor(item.gridSize);

        SubscribeClicks();
        GameManager.Instance.GameModeManager.SetMode(GameMode.Building);
    }

    public void SetRemoveMode()
    {
        gameObject.SetActive(true);

        currentMode = BuildingMode.Removal;

        ResetPreview();
        SetupCursor(Vector2Int.one, true, false);

        SubscribeClicks();
        GameManager.Instance.GameModeManager.SetMode(GameMode.Building);
    }

    private void HandleLeftClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;

        switch(currentMode)
        {
            case BuildingMode.Placement:
                TryPlaceAtCursor();
                break;
            case BuildingMode.Removal:
                TryRemoveAtCursor();
                break;
            default:
                break;
        }
    }

    private void HandleRightClick()
    {
        if (GameManager.Instance.Input.IsPointedOverUI()) return;
        CancelPlacement();
    }

    private void TryPlaceAtCursor()
    {
        if (!selectedItem || !RaycastGrid(out RaycastHit hit)) return;

        GameObject obj = PlaceItem(selectedItem, hit.point);

        if (obj.TryGetComponent<PlaceableObject>(out var placeableObject))
        {
            placeableObject.Initialize(selectedItem);
            placeableObject.Place();
        }

        if (obj.TryGetComponent<Animal>(out var animal))
        {
            animal.Initialize(AnimalType.Resident);
        }

        if (selectedItem.isUnique)
            CancelPlacement();
    }

    private void TryRemoveAtCursor()
    {
        if (!RaycastGrid(out RaycastHit hit)) return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);
        if (objectGrid.TryGetObject(cellPos, out var obj)) RemoveObject(obj);
    }

    public void RemoveObject(GameObject obj)
    {
        if (!obj) return;

        if (obj.TryGetComponent<PlaceableObject>(out var placeableObject))
        {
            if (placeableObject.ObjectData == null)
            {
                Debug.LogWarning("PlaceableObject has no ObjectData assigned.");
            }
            Vector3Int cellPos = grid.WorldToCell(obj.transform.position);
            objectGrid.Remove(cellPos, placeableObject.ObjectData.gridSize);
        }
        Destroy(obj);
    }

    public GameObject PlaceItem(ObjectData data, Vector3 position)
    {
        GameObject obj;

        if (data.isGridItem)
        {
            Vector3 spawnPos = grid.GetCellCenterWorld(grid.WorldToCell(position));
            Vector3Int cellPos = grid.WorldToCell(spawnPos);

            if (!IsInsideGrid(cellPos, data.gridSize)) return null;
            if (!CanPlaceObject(cellPos, data.gridSize)) return null;

            obj = Instantiate(data.prefab, spawnPos, Quaternion.identity, objectsParent);
            objectGrid.Register(cellPos, obj, data.gridSize);
            return obj;
        }
        else
        {
            obj = Instantiate(data.prefab, position, Quaternion.identity, objectsParent);
            return obj;
        }
    }

    private void MovePreview(Vector3 position)
    {
        if (previewObject) previewObject.transform.position = position;
    }

    private void UpdateCursor(Vector3Int cellPos)
    {
        if (!cursor) return;

        cursor.UpdatePosition(grid.CellToWorld(cellPos));

        if (currentMode == BuildingMode.Placement)
            cursor.UpdateColor(CanPlaceObject(cellPos, selectedItem?.gridSize ?? Vector2Int.one));
    }

    private void CancelPlacement()
    {
        gameObject.SetActive(false);

        ResetPreview();
        selectedItem = null;
        currentMode = BuildingMode.None;
        UnsubscribeClicks();
        if (cursor) cursor.SetActive(false);

        GameManager.Instance.GameModeManager.SetMode(GameMode.Normal);
    }

    private void CreatePreview(GameObject prefab)
    {
        if (prefab) previewObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);
    }

    private void ResetPreview()
    {
        if (previewObject) Destroy(previewObject);
    }

    private void SetupCursor(Vector2Int size, bool active = true, bool color = true)
    {
        if (!cursor) return;

        cursor.SetActive(active);
        cursor.SetSize(size);
        cursor.UpdateColor(color);
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

    private void SubscribeClicks()
    {
        UnsubscribeClicks();
        var input = GameManager.Instance.Input;
        input.OnLeftClick += HandleLeftClick;
        input.OnRightClick += HandleRightClick;
    }

    private void UnsubscribeClicks()
    {
        var input = GameManager.Instance.Input;
        input.OnLeftClick -= HandleLeftClick;
        input.OnRightClick -= HandleRightClick;
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

    public void Remove(Vector3Int cellPos, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
                grid.Remove(new Vector3Int(cellPos.x + x, cellPos.y, cellPos.z + z));
    }

    public bool IsOccupied(Vector3Int cell) => grid.ContainsKey(cell);
    public bool TryGetObject(Vector3Int cell, out GameObject obj) => grid.TryGetValue(cell, out obj);
}
