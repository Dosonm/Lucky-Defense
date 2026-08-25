using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    public event Action<int> OnGoldChanged;
    public event Action<int> OnSummonStoneChanged;

    [Header("Canvas")]
    [SerializeField] private TextMeshProUGUI goldTmp;
    [SerializeField] private TextMeshProUGUI summonStoneTmp;
    [SerializeField] private TextMeshProUGUI populationTmp;

    [Header("Reward Popup")]
    [SerializeField] private RewardPopupEffect rewardPopupPrefab;
    [SerializeField] private RectTransform goldPopupAnchor;
    [SerializeField] private Sprite goldIcon;
    [SerializeField] private RectTransform stonePopupAnchor;
    [SerializeField] private Sprite stoneIcon;

    [Header("Element")]
    [SerializeField] private int StartGold;
    [SerializeField] private int StartStone;
    public int Gold {get; private set;}

    public int SummonStone {get; private set;}

    public int maxPopulation;
    public int Population {get; private set;}

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
        Gold = StartGold;
        SummonStone = StartStone;

        UpdateGoldUi();
        UpdateSummonStoneUi();
        UpdatePopulationUi();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        UpdateGoldUi();
        PlayGoldPopup(amount);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount)
            return false;

        Gold -= amount;
        UpdateGoldUi();
        return true;
    }

    public void AddSummonStone(int amount)
    {
        SummonStone += amount;
        UpdateSummonStoneUi();
        PlayStonePopup(amount);
    }

    public bool SpendSummonStone(int amount)
    {
        if (SummonStone < amount)
            return false;

        SummonStone -= amount;
        UpdateSummonStoneUi();
        return true;
    }
    public void AddPopulation(int amount)
    {
        Population += amount;
        UpdatePopulationUi();
    }

    public bool RemovePopulation(int amount)
    {
        if (Population <= 0)
            return false;

        Population -= amount;
        UpdatePopulationUi();
        return true;
    }

    private void UpdateGoldUi()
    {
        goldTmp.text = $"{Gold}";
        OnGoldChanged?.Invoke(Gold);
    }

    private void UpdateSummonStoneUi()
    {
        summonStoneTmp.text = $"{SummonStone}";
        OnSummonStoneChanged?.Invoke(SummonStone);
    }

    private void UpdatePopulationUi()
    {
        populationTmp.text = $"{Population} / {maxPopulation}";
    }
    private void PlayGoldPopup(int amount)
    {
        RewardPopupEffect popup = PoolManager.Instance.Get<RewardPopupEffect>(
            rewardPopupPrefab.gameObject, Vector3.zero, Quaternion.identity);

        popup.transform.SetParent(goldPopupAnchor, false);
        popup.Play($"+{amount}", goldIcon);
    }

    private void PlayStonePopup(int amount)
    {
        RewardPopupEffect popup = PoolManager.Instance.Get<RewardPopupEffect>(
            rewardPopupPrefab.gameObject, Vector3.zero, Quaternion.identity);

        popup.transform.SetParent(stonePopupAnchor, false);
        popup.Play($"+{amount}", stoneIcon);
    }
}