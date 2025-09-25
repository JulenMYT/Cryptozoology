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
    [SerializeField] private ItemUIManager itemUIManager;

    private ItemData selectedItem;
    private GameObject previewObject;
    private Vector3Int previousCell;
    private Dictionary<Vector3Int, GameObject> placedObjects = new();
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

    public void SelectItem(ItemData item)
    {
        selectedItem = item;
        removeMode = false;

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
        if (!IsInsideGrid(cellPos, selectedItem? selectedItem.gridSize : Vector2Int.one)) return;

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
        if (!placedObjects.TryGetValue(cellPos, out var obj)) return;

        Destroy(obj);
        placedObjects.Remove(cellPos);
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

        if (obj.TryGetComponent<PlantBehaviour>(out var plant) && selectedItem is PlantDataSO plantData)
            plant.Initialize(plantData);

        if (selectedItem.category != ItemCategory.Animal)
            RegisterObject(cellPos, obj, selectedItem.gridSize);

        ItemEvents.OnItemAdded?.Invoke(selectedItem);
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
