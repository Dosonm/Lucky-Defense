using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.ObjectModel;

public enum AttackTypes
{
    Closer,
    Ranger
}

public enum SummonsTypes
{
    Archer,
    Ice,
    Barbarian,
    Thrower,
    Bandit,
    Demon,
    Sandman,
    Paladin,
    Robot,
    Ranger,
    Wolf,
    Eagle,
    Hunter,
    Tree,
    Robot2,
    Sheriff,
    Storm,
    Tiger,
    Ninja,
    Vain
}

public enum SummonsGrade
{
    Normal,
    Rare,
    Hero,
    Legend,
    Myth
}

public class Summons : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private GridManager gridManager;
    private SpriteRenderer sr;

    public AttackTypes attackType;
    public SummonsTypes summonsType;
    public SummonsGrade summonsGrade;

    [Header("Myth Compound")]
    [SerializeField] private string Name;
    [SerializeField] private List<SummonsTypes> requiredUnits = new();

    //requiredUnits을 외부에서 볼 수는 있지만 수정은 못하게 (get private set으로는 list의 add,remove와 같은 걸 막을 수 없음)
    public ReadOnlyCollection<SummonsTypes> RequiredUnits
        => requiredUnitsReadOnly ??= requiredUnits.AsReadOnly();

    //캐싱용 변수 (매번 requiredUnits.AsReadOnly() 이걸 실행하지않기 위해서)
    private ReadOnlyCollection<SummonsTypes> requiredUnitsReadOnly;


    [Header("Attack")]
    [SerializeField] private GameObject projectile;
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooltime;
    [SerializeField] private int damage;
    [SerializeField] private int critChance;

    [Header("Grid")]
    [SerializeField] private float snapDuration = 0.2f;
    

    [Header("Element")]
    private LayerMask monsterLayer;
    private float cooldownTimer;

    private bool isDragging;
    private bool draggedThisPress;
    private Vector3 dragOffset;

    private readonly List<Summons> dragGroupmates = new();
    private readonly List<Vector3> dragGroupmateOffsets = new();

    public GridCell CurrentCell { get; set; }

    public int EffectiveDamage => Mathf.RoundToInt(damage * GameManager.Instance.GetDamageMultiplier(summonsGrade));

    public LineRenderer rangeIndicator;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rangeIndicator = CreateRangeIndicator();
    }

    private LineRenderer CreateRangeIndicator()
    {
        GameObject indicatorObject = new GameObject("RangeIndicator");
        indicatorObject.transform.SetParent(transform, false);

        LineRenderer line = indicatorObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.material = GridCell.LineMaterial;
        line.startColor = line.endColor = new Color(0.85f, 0.95f, 1f, .5f);
        line.startWidth = line.endWidth = 0.02f;
        line.sortingLayerName = "Object";
        line.sortingOrder = 100;

        const int segments = 48;
        line.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i / (float)segments * Mathf.PI * 2f;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * attackRange);
        }

        line.enabled = false;
        return line;
    }

    private void Start()
    {
        gridManager = GridManager.Instance;

        monsterLayer = LayerMask.GetMask("Monster");

        if (CurrentCell == null)
        {
            GridCell nearestCell = gridManager.GetNearestCell(transform.position);
            GridCell targetCell = nearestCell.CanAccept(summonsType) ? nearestCell : gridManager.GetCellForSpawn(summonsType);

            if (targetCell != null)
                targetCell.AddOccupant(this);
        }
    }

    private void Update()
    {

        if (isDragging)
        {
            rangeIndicator.transform.position = transform.position;
            return;
        }

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer > 0f)
            return;

        Monster target = FindTarget();
        if (target == null)
        {
            cooldownTimer = .2f;
            return;
        }

        Attack(target);
        cooldownTimer = attackCooltime;
    }

    public Vector3 GetAttackOrigin()
    {
        if (isDragging || CurrentCell == null)
            return transform.position;

        return CurrentCell.transform.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        gridManager.NotifyPointerDown();

        rangeIndicator.enabled = true;
        gridManager.ShowGrid();

        draggedThisPress = false;
        dragOffset = transform.position - GetMouseWorldPosition(eventData.position);

        dragGroupmates.Clear();
        dragGroupmateOffsets.Clear();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        rangeIndicator.enabled = false;
        gridManager.HideGrid();

        isDragging = false;
        foreach (Summons summons in dragGroupmates)
            summons.isDragging = false;

        if (!draggedThisPress)
            gridManager.RequestSelect(this);

        GridCell originCell = CurrentCell;
        if (originCell == null)
            return;

        GridCell targetCell = gridManager.GetNearestCell(transform.position);

        if (targetCell == originCell)
        {
            originCell.ApplyLayout();
            return;
        }

        if (!gridManager.MoveGroupToCell(originCell, targetCell))
            originCell.ApplyLayout();

        dragGroupmates.Clear();
        dragGroupmateOffsets.Clear();
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (!draggedThisPress)
        {
            draggedThisPress = true;
            isDragging = true;

            if (CurrentCell != null)
            {
                foreach (Summons summons in CurrentCell.Occupants)
                {
                    if (summons == this)
                        continue;

                    summons.isDragging = true;
                    dragGroupmates.Add(summons);
                    dragGroupmateOffsets.Add(summons.transform.position - transform.position);
                }
            }
        }

        Vector3 newPosition = GetMouseWorldPosition(eventData.position) + dragOffset;
        transform.position = newPosition;

        for (int i = 0; i < dragGroupmates.Count; i++)
            dragGroupmates[i].transform.position = newPosition + dragGroupmateOffsets[i];
    }

    public void SetRangeIndicatorVisible(bool visible)
    {
        rangeIndicator.enabled = visible;
    }

    public void ResetForReuse()
    {
        transform.DOKill();

        rangeIndicator.enabled = false;

        isDragging = false;
        draggedThisPress = false;
        dragGroupmates.Clear();
        dragGroupmateOffsets.Clear();

        cooldownTimer = 0f;
        CurrentCell = null;
    }

    public void SnapTo(Vector3 position)
    {
        transform.DOKill();
        transform.DOMove(position, snapDuration);
    }

    private Vector3 GetMouseWorldPosition(Vector2 screenPos)
    {
        Vector3 mouseScreen = screenPos;
        mouseScreen.z = -Camera.main.transform.position.z;
        Vector3 world = Camera.main.ScreenToWorldPoint(mouseScreen);
        world.z = transform.position.z;
        return world;
    }

    private Monster FindTarget()
    {
        Collider2D hit = Physics2D.OverlapCircle(GetAttackOrigin(), attackRange, monsterLayer);
        return hit != null ? hit.GetComponent<Monster>() : null;
    }

    private void Attack(Monster target)
    {
        sr.flipX = target.transform.position.x > transform.position.x;

        switch (attackType)
        {
            case AttackTypes.Closer:
                DoDamage(target, EffectiveDamage);
                break;

            case AttackTypes.Ranger:
                Vector2 direction = (Vector2)target.transform.position - (Vector2)transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.Euler(0f, 0f, angle - 90);

                Projectile projectileInstance = PoolManager.Instance.Get<Projectile>(projectile, transform.position, rotation);
                projectileInstance.Initialize(this, target, EffectiveDamage);
                break;
        }
    }
    public void DoDamage(Monster target, int damage)
    {
        int totalDmg = damage;

        bool crit = IsCrit();
        if(crit)
            totalDmg *= (int)1.5f;

        target.GetComponent<MonsterStat>().TakeDamage(totalDmg, crit);
        
    }

    public bool IsCrit()
    {
        return UnityEngine.Random.Range(0,100) < critChance;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackOrigin(), attackRange);
    }

    private void OnDestroy() 
    {
        transform.DOKill();
    }
}