using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private GridManager gridManager;
    private CurrencyManager currencyManager;

    [SerializeField] private Summons[] unitPrefabs;

    [Header("Canvas")]
    [SerializeField] private TextMeshProUGUI waveTmp;
    [SerializeField] private TextMeshProUGUI timeTmp;
    [SerializeField] private TextMeshProUGUI countTmp;
    [SerializeField] private TextMeshProUGUI costTmp;

    [Header("Gacha")]
    private const int RareGachaCost = 1;
    private const float RareGachaChance = 0.6f;
    private const int HeroGachaCost = 1;
    private const float HeroGachaChance = 0.25f;
    private const int LegendGachaCost = 2;
    private const float LegendGachaChance = 0.13f;

    [Header("Element")]
    public int CurrentEnemyCount;
    public int MaxEnemyCount = 100;
    private int StartSpawnCost = 10;
    private int CurrentSpawnCount;
    public int SpawnCost;
    public int SummonLevel { get; private set; } = 1;
    public bool IsSummonLevelMax => SummonLevel >= MaxSummonLevel;

    private const int MaxSummonLevel = 10;
    private const int SummonLevelUpCost = 100;

    [Header("Attack Upgrade")]
    public int lowGradeUpgradeLevel { get; private set; }
    public int heroUpgradeLevel { get; private set; }
    public int highGradeUpgradeLevel { get; private set; }

    private const int LowGradeUpgradeBaseCost = 30;
    private const int LowGradeUpgradeCostStep = 20;
    private const int HeroUpgradeBaseCost = 50;
    private const int HeroUpgradeCostStep = 30;
    private const int HighGradeUpgradeBaseCost = 2;
    private const int HighGradeUpgradeCostStep = 1;

    private const float UpgradeDamageMultiplierPerLevel = 0.05f;

    public event System.Action OnUpgradeChanged;

    private static readonly float[] SummonWeightsAtLevel1 = { 90f, 6f, 3f, 1f };
    private static readonly float[] SummonWeightsAtLevel10 = { 60f, 25f, 10f, 5f };

    public bool IsGameOver { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        gridManager = GridManager.Instance;
        currencyManager = CurrencyManager.Instance;

        UpdateCostUi(GetNextSpawnCost());
    }

    private int GetNextSpawnCost() => StartSpawnCost + CurrentSpawnCount * 5;

    public void AddEnemyCount()
    {
        if (IsGameOver)
            return;

        CurrentEnemyCount++;
        UpdateCountUi();

        if (CurrentEnemyCount >= MaxEnemyCount)
            GameOver();
    }

    public void RemoveEnemyCount()
    {
        if (CurrentEnemyCount > 0)
        {
            CurrentEnemyCount--;
            UpdateCountUi();
        }
    }

    private void GameOver()
    {
        IsGameOver = true;
        Time.timeScale = 0f;
        Debug.Log("Game Over: enemy count reached " + MaxEnemyCount);
    }

    public void SpawnUnit()
    {
        if (currencyManager.Population >= currencyManager.maxPopulation)
            return;

        SummonsGrade grade = RollSummonGrade();
        Summons prefab = GetRandomPrefabForGrade(grade);
        if (prefab == null)
        {
            return;
        }

        GridCell targetCell = gridManager.GetCellForSpawn(prefab.summonsType);
        if (targetCell == null)
            return;

        SpawnCost = GetNextSpawnCost();
        if (!currencyManager.SpendGold(SpawnCost)) 
            return;

        CurrentSpawnCount++;
        UpdateCostUi(GetNextSpawnCost());
        currencyManager.AddPopulation(1);

        Summons instance = PoolManager.Instance.Get<Summons>(prefab.gameObject, targetCell.transform.position, Quaternion.identity);
        instance.ResetForReuse();
        targetCell.AddOccupant(instance);
    }

    public Summons GetPrefabForType(SummonsTypes type)
    {
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            if (unitPrefabs[i].summonsType == type)
                return unitPrefabs[i];
        }

        return null;
    }

    public Summons GetRandomPrefabForGrade(SummonsGrade grade)
    {
        int count = 0;
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            if (unitPrefabs[i].summonsGrade == grade)
                count++;
        }

        if (count == 0)
            return null;

        int pick = Random.Range(0, count);
        for (int i = 0; i < unitPrefabs.Length; i++)
        {
            if (unitPrefabs[i].summonsGrade != grade)
                continue;

            if (pick == 0)
                return unitPrefabs[i];

            pick--;
        }

        return null;
    }

    private SummonsGrade RollSummonGrade()
    {
        float t = Mathf.Clamp01((SummonLevel - 1) / (float)(MaxSummonLevel - 1));

        float total = 0f;
        float[] weights = new float[SummonWeightsAtLevel1.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = Mathf.Lerp(SummonWeightsAtLevel1[i], SummonWeightsAtLevel10[i], t);
            total += weights[i];
        }

        float roll = Random.value * total;
        float cumulative = 0f;
        for (int i = 0; i < weights.Length - 1; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
                return (SummonsGrade)i;
        }

        return (SummonsGrade)(weights.Length - 1);
    }

    public void LevelUpSummon()
    {
        if (SummonLevel >= MaxSummonLevel)
            return;

        if (!currencyManager.SpendGold(SummonLevelUpCost))
            return;

        SummonLevel++;
        OnUpgradeChanged?.Invoke();
    }

    public int GetLowGradeUpgradeCost() => LowGradeUpgradeBaseCost + LowGradeUpgradeCostStep * lowGradeUpgradeLevel;
    public int GetHeroUpgradeCost() => HeroUpgradeBaseCost + HeroUpgradeCostStep * heroUpgradeLevel;
    public int GetHighGradeUpgradeCost() => HighGradeUpgradeBaseCost + HighGradeUpgradeCostStep * highGradeUpgradeLevel;

    public void UpgradeLowGradeDamage()
    {
        if (!currencyManager.SpendGold(GetLowGradeUpgradeCost()))
            return;

        lowGradeUpgradeLevel++;
        OnUpgradeChanged?.Invoke();
    }

    public void UpgradeHeroDamage()
    {
        if (!currencyManager.SpendGold(GetHeroUpgradeCost()))
            return;

        heroUpgradeLevel++;
        OnUpgradeChanged?.Invoke();
    }

    public void UpgradeHighGradeDamage()
    {
        if (!currencyManager.SpendSummonStone(GetHighGradeUpgradeCost()))
            return;

        highGradeUpgradeLevel++;
        OnUpgradeChanged?.Invoke();
    }

    public float GetDamageMultiplier(SummonsGrade grade)
    {
        int level = grade switch
        {
            SummonsGrade.Normal or SummonsGrade.Rare => lowGradeUpgradeLevel,
            SummonsGrade.Hero => heroUpgradeLevel,
            SummonsGrade.Legend or SummonsGrade.Myth => highGradeUpgradeLevel,
            _ => 0
        };

        return 1f + level * UpgradeDamageMultiplierPerLevel;
    }

    public void SummonRareGacha() => TryGachaSummon(SummonsGrade.Rare, RareGachaCost, RareGachaChance);
    public void SummonHeroGacha() => TryGachaSummon(SummonsGrade.Hero, HeroGachaCost, HeroGachaChance);
    public void SummonLegendGacha() => TryGachaSummon(SummonsGrade.Legend, LegendGachaCost, LegendGachaChance);

    private void TryGachaSummon(SummonsGrade grade, int stoneCost, float successChance)
    {
        if (currencyManager.Population >= currencyManager.maxPopulation) return;

        if (!currencyManager.SpendSummonStone(stoneCost)) return;

        if (Random.value >= successChance) return;

        Summons prefab = GetRandomPrefabForGrade(grade);
        if (prefab == null) return;

        GridCell targetCell = gridManager.GetCellForSpawn(prefab.summonsType);
        if (targetCell == null) return;

        currencyManager.AddPopulation(1);

        Summons instance = PoolManager.Instance.Get<Summons>(prefab.gameObject, targetCell.transform.position, Quaternion.identity);
        instance.ResetForReuse();
        targetCell.AddOccupant(instance);
    }

    public void UpdateWaveUi()
    {
        waveTmp.text = $"WAVE {EnemySpawner.currentWave + 1}";
    }

    public void UpdateTimeUi(int remainingSeconds)
    {
        timeTmp.text = $"{remainingSeconds / 60:00}:{remainingSeconds % 60:00}";
    }

    private void UpdateCountUi()
    {
        countTmp.text = $"{CurrentEnemyCount} / {MaxEnemyCount}";
    }

    private void UpdateCostUi(int cost)
    {
        costTmp.text = $"{cost}";
    }

    public void OpenUiBtn(GameObject gameObject)
    {
        gameObject.SetActive(true);
    }

    public void CloseUiBtn(GameObject gameObject)
    {
        gameObject.SetActive(false);
    }
}