using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    private CurrencyManager currencyManager;

    public int Width => width;
    public int Height => height;

    [SerializeField] private GridCell cellPrefab;
    public Material lineMaterial;
    [SerializeField] private int width = 5;
    [SerializeField] private int height = 5;

    [Header("Selection")]
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private Button compoundButton;
    [SerializeField] private Button sellButton;

    private Summons selectedSummons;

    [Header("Bounds (World Space)")]
    [SerializeField] private float minX = -1.5f;
    [SerializeField] private float maxX = 1.5f;
    [SerializeField] private float minY = -1.2f;
    [SerializeField] private float maxY = 1.4f;

    private GridCell[,] cells;
    private float cellWidth;
    private float cellHeight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cellWidth = (maxX - minX) / width;
        cellHeight = (maxY - minY) / height;

        BuildGrid();

        if (selectionUI != null)
            selectionUI.SetActive(false);
    }

    private void Start() 
    {
        currencyManager = CurrencyManager.Instance;
    }

    private void Update()
    {
        if (selectedSummons == null)
            return;

        selectedSummons.rangeIndicator.transform.position = selectedSummons.GetAttackOrigin();

        if (selectionUI != null)
            selectionUI.transform.position = GetSelectionAnchorPosition(selectedSummons);

        UpdateCompoundButton();
        UpdateSellButton();
    }

    private void BuildGrid()
    {
        cells = new GridCell[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = CellToWorld(x, y);
                GridCell cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.Initialize(x, y, cellWidth, cellHeight);
                cells[x, y] = cell;
            }
        }
    }

    public Vector3 CellToWorld(int x, int y)
    {
        float worldX = minX + cellWidth * (x + 0.5f);
        float worldY = minY + cellHeight * (y + 0.5f);
        return new Vector3(worldX, worldY, 0f);
    }

    public GridCell GetCell(int x, int y) => cells[x, y];

    public void ShowGrid()
    {
        foreach (GridCell cell in cells)
            cell.SetBorderVisible(true);
    }

    public void HideGrid()
    {
        foreach (GridCell cell in cells)
            cell.SetBorderVisible(false);
    }

    // 클릭 확정(=드래그 없이 뗀 순간) 시 Summons가 호출.
    public void RequestSelect(Summons summons)
    {
        SetSelected(summons);
    }

    // 어떤 유닛이든 눌리는 순간(드래그 시작), 또는 빈 셀 클릭 시 호출.
    public void NotifyPointerDown()
    {
        SetSelected(null);
    }

    private void SetSelected(Summons summons)
    {
        if (selectedSummons == summons)
            return;

        if (selectedSummons != null)
            selectedSummons.SetRangeIndicatorVisible(false);

        selectedSummons = summons;

        if (selectedSummons != null)
        {
            selectedSummons.SetRangeIndicatorVisible(true);
            if (selectionUI != null)
            {
                selectionUI.transform.position = GetSelectionAnchorPosition(selectedSummons);
                selectionUI.SetActive(true);
            }
        }
        else if (selectionUI != null)
        {
            selectionUI.SetActive(false);
        }

        UpdateCompoundButton();
        UpdateSellButton();
        UpdateGridVisibility();
    }

    private void UpdateCompoundButton()
    {
        if (compoundButton == null)
            return;

        bool canCompound = selectedSummons != null
            && selectedSummons.CurrentCell != null
            && selectedSummons.summonsGrade < SummonsGrade.Myth
            && selectedSummons.CurrentCell.CanCompound;

        compoundButton.interactable = canCompound;
    }

    private void UpdateSellButton()
    {
        if (sellButton == null)
            return;

        bool canSell = selectedSummons != null
            && selectedSummons.CurrentCell != null
            && selectedSummons.summonsGrade < SummonsGrade.Myth;

        sellButton.interactable = canSell;
    }

    public void CompoundSelected()
    {
        if (selectedSummons == null || selectedSummons.CurrentCell == null) return;

        GridCell cell = selectedSummons.CurrentCell;
        if (!cell.CanCompound) return;

        SummonsGrade nextGrade = cell.Occupants[0].summonsGrade + 1;

        Summons prefab = GameManager.Instance.GetRandomPrefabForGrade(nextGrade);
        if (prefab == null) return;

        List<Summons> group = GetGroup(cell);

        foreach (Summons summons in group)
        {
            cell.RemoveOccupant(summons);
            summons.ResetForReuse();
            PoolManager.Instance.Release(summons.gameObject);
        }

        currencyManager.RemovePopulation(group.Count - 1);

        GridCell mergeableCell = FindMergeableCell(prefab.summonsType);
        GridCell targetCell = mergeableCell != null ? mergeableCell : cell;
        Summons instance = PoolManager.Instance.Get<Summons>(prefab.gameObject, targetCell.transform.position, Quaternion.identity);
        instance.ResetForReuse();
        targetCell.AddOccupant(instance);

        SetSelected(instance);
    }

    public void SellSelected()
    {
        if (selectedSummons == null || selectedSummons.CurrentCell == null)
            return;

        GridCell cell = selectedSummons.CurrentCell;
        Summons summons = selectedSummons;
        SummonsGrade summonsGrade = summons.summonsGrade;

        SetSelected(null);

        cell.RemoveOccupant(summons);
        summons.ResetForReuse();
        PoolManager.Instance.Release(summons.gameObject);

        currencyManager.RemovePopulation(1);

        switch (summonsGrade)
        {
            case SummonsGrade.Normal:
                currencyManager.AddGold(GameManager.Instance.SpawnCost / 2);
                break;

            case SummonsGrade.Rare:
                currencyManager.AddSummonStone(1);
                break;

            case SummonsGrade.Hero:
                currencyManager.AddSummonStone(2);
                break;

            case SummonsGrade.Legend:
                currencyManager.AddSummonStone(4);
                break;
        }
    }

    public List<Summons> FindMythMaterials(IList<SummonsTypes> requiredUnits)
    {
        if (requiredUnits == null || requiredUnits.Count == 0)
            return null;

        List<Summons> materials = new();

        foreach (SummonsTypes type in requiredUnits)
        {
            Summons found = FindUnitOfType(type, materials);
            if (found == null)
                return null;

            materials.Add(found);
        }

        return materials;
    }

    public bool HasUnitOfType(SummonsTypes type) => FindUnitOfType(type, null) != null;

    private Summons FindUnitOfType(SummonsTypes type, List<Summons> exclude)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                foreach (Summons summons in cells[x, y].Occupants)
                {
                    if (summons.summonsType == type && (exclude == null || !exclude.Contains(summons)))
                        return summons;
                }
            }
        }

        return null;
    }

    public bool CanCompoundMyth(IList<SummonsTypes> requiredUnits)
    {
        List<Summons> materials = FindMythMaterials(requiredUnits);
        return materials != null;
    }

    public void CompoundMyth(Summons mythPrefab)
    {
        List<Summons> materials = FindMythMaterials(mythPrefab.RequiredUnits);
        if (materials == null)
            return;

        if (selectedSummons != null && materials.Contains(selectedSummons))
            SetSelected(null);

        GridCell targetCell = materials[0].CurrentCell;

        foreach (Summons material in materials)
        {
            material.CurrentCell.RemoveOccupant(material);
            material.ResetForReuse();
            PoolManager.Instance.Release(material.gameObject);
        }

        currencyManager.RemovePopulation(materials.Count - 1);

        GridCell spawnCell = GetCellForSpawn(mythPrefab.summonsType);
        if (spawnCell == null)
            spawnCell = targetCell;

        Summons instance = PoolManager.Instance.Get<Summons>(mythPrefab.gameObject, spawnCell.transform.position, Quaternion.identity);
        instance.ResetForReuse();
        spawnCell.AddOccupant(instance);
    }

    private static Vector3 GetSelectionAnchorPosition(Summons summons)
    {
        return summons.CurrentCell != null ? summons.CurrentCell.transform.position : summons.transform.position;
    }

    private void UpdateGridVisibility()
    {
        if (selectedSummons != null)
            ShowGrid();
        else
            HideGrid();
    }

    public GridCell GetCellForSpawn(SummonsTypes type)
    {
        GridCell mergeableCell = FindMergeableCell(type);
        return mergeableCell != null ? mergeableCell : GetFirstFreeCell();
    }

    public GridCell FindMergeableCell(SummonsTypes type)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                GridCell cell = cells[x, y];
                if (!cell.IsEmpty && !cell.IsFull && cell.OccupantType == type)
                    return cell;
            }
        }

        return null;
    }
    public GridCell GetFirstFreeCell()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (cells[x, y].IsEmpty)
                    return cells[x, y];
            }
        }
        return null;
    }

    public GridCell GetNearestCell(Vector3 worldPosition)
    {
        int x = Mathf.Clamp(Mathf.FloorToInt((worldPosition.x - minX) / cellWidth), 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt((worldPosition.y - minY) / cellHeight), 0, height - 1);
        return cells[x, y];
    }

    public static List<Summons> GetGroup(GridCell cell)
    {
        return new List<Summons>(cell.Occupants);
    }

    // 같은 타입이고 합쳐서 3마리 이하면 합침. 그 외(다른 타입, 또는 같은 타입인데 자리 부족)는 스왑.
    public bool MoveGroupToCell(GridCell originCell, GridCell targetCell)
    {
        List<Summons> movingGroup = GetGroup(originCell);
        if (movingGroup.Count == 0)
            return false;

        SummonsTypes movingType = movingGroup[0].summonsType;

        bool canMerge = (targetCell.IsEmpty || targetCell.OccupantType == movingType)
            && targetCell.Occupants.Count + movingGroup.Count <= GridCell.MaxStack;

        if (canMerge)
        {
            foreach (Summons summons in movingGroup)
            {
                originCell.RemoveOccupant(summons);
                targetCell.AddOccupant(summons);
            }
            return true;
        }

        SwapGroups(originCell, targetCell);
        return true;
    }

    public void SwapGroups(GridCell cellA, GridCell cellB)
    {
        List<Summons> groupA = GetGroup(cellA);
        List<Summons> groupB = GetGroup(cellB);

        foreach (Summons summons in groupA)
            cellA.RemoveOccupant(summons);
        foreach (Summons summons in groupB)
            cellB.RemoveOccupant(summons);

        foreach (Summons summons in groupA)
            cellB.AddOccupant(summons);
        foreach (Summons summons in groupB)
            cellA.AddOccupant(summons);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 center = new((minX + maxX) / 2f, (minY + maxY) / 2f, 0f);
        Vector3 size = new(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}