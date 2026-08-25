using UnityEngine;

public class MonsterHealthBar : MonoBehaviour
{
    [SerializeField] private SpriteRenderer back;
    [SerializeField] private SpriteRenderer fill;

    private Monster monster;
    private MonsterStat monsterStat;
    private Vector3 initialFillScale;

    private void Awake()
    {
        monster = GetComponentInParent<Monster>();
        monsterStat = monster.GetComponent<MonsterStat>();
        initialFillScale = fill.transform.localScale;
    }

    private void OnEnable()
    {
        monsterStat.onHealthChanged += UpdateHealthBar;
        UpdateHealthBar();

        int order = monster.order;
        back.sortingOrder = order;
        fill.sortingOrder = order+1;
    }

    private void UpdateHealthBar()
    {
        float ratio = monsterStat.MaxHp > 0 ? (float)monsterStat.CurrentHp / monsterStat.MaxHp : 0f;
        ratio = Mathf.Clamp01(ratio);

        fill.transform.localScale = new Vector3(initialFillScale.x * ratio, initialFillScale.y, initialFillScale.z);
    }

    private void OnDisable()
    {
        monsterStat.onHealthChanged -= UpdateHealthBar;
    }
}