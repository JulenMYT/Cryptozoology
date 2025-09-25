using UnityEngine;
using System.Collections.Generic;

public class GridPlacementManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private Transform objectsParent;
    [SerializeField] private LayerMask gridLayer;
    [SerializeField] private PlacementCursor cursor;
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;

    private ItemData selectedItem;
    private GameObject previewObject;
    private Vector3Int previousCell;
    private Dictionary<Vector3Int, GameObject> placedObjects = new();

    private void OnEnable()
    {
        ItemEvents.OnItemSelected += SelectItem;
    }

    private void OnDisable()
    {
        ItemEvents.OnItemSelected -= SelectItem;
    }

    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        DestroyPreview();

        if (item.prefab != null)
        {
            previewObject = Instantiate(item.prefab, Vector3.zero, Quaternion.identity, objectsParent);
            if (previewObject.TryGetComponent<Collider>(out var col))
                col.enabled = false;
        }

        if (cursor != null)
        {
            cursor.SetActive(true);
            cursor.SetSize(item.gridSize);
        }
    }

    private void Update()
    {
        if (selectedItem == null)
        {
            if (cursor != null && cursor.IsActive())
                cursor.SetActive(false);
            return;
        }

        if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100f, gridLayer))
            return;

        Vector3Int cellPos = grid.WorldToCell(hit.point);

        if (!IsInsideGrid(cellPos, selectedItem.gridSize))
            return;

        Vector3 placePos = grid.GetCellCenterWorld(cellPos);

        if (cellPos != previousCell)
        {
            previousCell = cellPos;
            MovePreview(placePos);
            UpdateCursor(cellPos);
        }

        if (Input.GetMouseButtonDown(0))
            PlaceItem(cellPos, placePos);

        if (Input.GetMouseButtonDown(1))
            CancelPlacement();
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
        cursor.UpdateColor(CanPlaceObject(cellPos, selectedItem.gridSize));
    }

    private void PlaceItem(Vector3Int cellPos, Vector3 position)
    {
        if (!CanPlaceObject(cellPos, selectedItem.gridSize))
            return;

        GameObject obj = Instantiate(selectedItem.prefab, position, Quaternion.identity, objectsParent);
        RegisterObject(cellPos, obj, selectedItem.gridSize);
    }

    private void CancelPlacement()
    {
        DestroyPreview();
        selectedItem = null;

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
            {
                Vector3Int checkPos = new(cellPos.x + x, cellPos.y, cellPos.z + z);
                if (IsCellOccupied(checkPos))
                    return false;
            }
        return true;
    }

    private bool IsInsideGrid(Vector3Int cellPos, Vector2Int size)
    {
        return cellPos.x >= 0 && cellPos.z >= 0 &&
               cellPos.x + size.x <= gridWidth &&
               cellPos.z + size.y <= gridHeight;
    }

    private void RegisterObject(Vector3Int cellPos, GameObject obj, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
            for (int z = 0; z < size.y; z++)
            {
                Vector3Int pos = new(cellPos.x + x, cellPos.y, cellPos.z + z);
                placedObjects[pos] = obj;
            }
    }

    private bool IsCellOccupied(Vector3Int cell) => placedObjects.ContainsKey(cell);
}
