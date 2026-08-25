using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GridCell : MonoBehaviour, IPointerDownHandler
{
    public const int MaxStack = 3;

    private static Material lineMaterial;
    public static Material LineMaterial
    {
        get
        {
            if (lineMaterial == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                lineMaterial = new Material(shader != null ? shader : Shader.Find("Unlit/Color"));
            }
            return lineMaterial;
        }
    }

    public int X { get; private set; }
    public int Y { get; private set; }

    public List<Summons> Occupants { get; } = new();
    public bool IsEmpty => Occupants.Count == 0;
    public bool IsFull => Occupants.Count >= MaxStack;
    public SummonsTypes? OccupantType => IsEmpty ? null : Occupants[0].summonsType;

    public bool CanCompound
    {
        get
        {
            if (!IsFull)
                return false;

            SummonsGrade grade = Occupants[0].summonsGrade;
            if (grade >= SummonsGrade.Legend)
                return false;

            for (int i = 1; i < Occupants.Count; i++)
            {
                if (Occupants[i].summonsGrade != grade)
                    return false;
            }

            return true;
        }
    }

    private Vector3 cellSize = Vector3.one;
    private LineRenderer border;

    public void Initialize(int x, int y, float width, float height)
    {
        X = x;
        Y = y;
        cellSize = new Vector3(width, height, 1f);

        border = CreateBorder();

        BoxCollider2D clickArea = GetComponent<BoxCollider2D>();
        if (clickArea == null)
            clickArea = gameObject.AddComponent<BoxCollider2D>();
        clickArea.size = new Vector2(cellSize.x, cellSize.y);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsEmpty)
            GridManager.Instance.NotifyPointerDown();
        else
            GridManager.Instance.RequestSelect(Occupants[0]);
    }

    private LineRenderer CreateBorder()
    {
        GameObject borderObject = new GameObject("Border");
        borderObject.transform.SetParent(transform, false);

        LineRenderer line = borderObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.material = LineMaterial;
        line.startWidth = line.endWidth = Mathf.Min(cellSize.x, cellSize.y) * 0.03f;
        line.sortingLayerName = "Object";
        line.sortingOrder = 90;

        float halfWidth = cellSize.x * 0.45f;
        float halfHeight = cellSize.y * 0.45f;
        line.positionCount = 4;
        line.SetPositions(new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
        });

        line.enabled = false;
        return line;
    }

    public void SetBorderVisible(bool visible)
    {
        if (border == null)
            return;

        Color color = new Color(0.1f, 0.45f, 0.15f, .5f);
        border.startColor = border.endColor = color;
        border.enabled = visible;
    }

    public bool CanAccept(SummonsTypes type)
    {
        if (IsEmpty)
            return true;

        return OccupantType == type && !IsFull;
    }

    public void AddOccupant(Summons summons)
    {
        Occupants.Add(summons);
        summons.CurrentCell = this;
        ApplyLayout();
    }

    public void RemoveOccupant(Summons summons)
    {
        Occupants.Remove(summons);
        ApplyLayout();
    }

    public Vector3 GetSlotPosition(int index)
    {
        return transform.position + GetSlotOffset(index, Occupants.Count);
    }

    private Vector3 GetSlotOffset(int index, int count)
    {
        float offsetX = cellSize.x * 0.22f;
        float offsetY = cellSize.y * 0.18f;

        switch (count)
        {
            case 1:
                return Vector3.zero;

            case 2:
                return index == 0
                    ? new Vector3(-offsetX, -offsetY, 0f)
                    : new Vector3(offsetX, -offsetY, 0f);

            default:
                return index switch
                {
                    0 => new Vector3(0f, offsetY, 0f),
                    1 => new Vector3(-offsetX, -offsetY, 0f),
                    _ => new Vector3(offsetX, -offsetY, 0f),
                };
        }
    }

    public void ApplyLayout()
    {
        for (int i = 0; i < Occupants.Count; i++)
            Occupants[i].SnapTo(GetSlotPosition(i));
    }

    /* private void OnDrawGizmos()
    {
        Gizmos.color = IsEmpty ? Color.green : (IsFull ? Color.red : Color.yellow);
        Gizmos.DrawWireCube(transform.position, cellSize * 0.9f);
    } */
}