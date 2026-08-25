using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI GoldTmp;
    [SerializeField] private TextMeshProUGUI StoneTmp;
    [SerializeField] private TextMeshProUGUI UgCost1Tmp;
    [SerializeField] private TextMeshProUGUI UgCost2Tmp;
    [SerializeField] private TextMeshProUGUI UgCost3Tmp;
    [SerializeField] private TextMeshProUGUI Ug1LvTmp;
    [SerializeField] private TextMeshProUGUI Ug2LvTmp;
    [SerializeField] private TextMeshProUGUI Ug3LvTmp;
    [SerializeField] private TextMeshProUGUI ProbabilityLvTmp;
    [SerializeField] private Button ProbabilityLevelUpButton;

    private void OnEnable()
    {
        CurrencyManager currencyManager = CurrencyManager.Instance;

        UpdateGoldText(currencyManager.Gold);
        UpdateStoneText(currencyManager.SummonStone);
        UpdateUpgradeCostTexts();

        currencyManager.OnGoldChanged += UpdateGoldText;
        currencyManager.OnSummonStoneChanged += UpdateStoneText;
        GameManager.Instance.OnUpgradeChanged += UpdateUpgradeCostTexts;
    }

    private void OnDisable()
    {
        CurrencyManager currencyManager = CurrencyManager.Instance;
        if (currencyManager != null)
        {
            currencyManager.OnGoldChanged -= UpdateGoldText;
            currencyManager.OnSummonStoneChanged -= UpdateStoneText;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.OnUpgradeChanged -= UpdateUpgradeCostTexts;
    }

    private void UpdateGoldText(int gold)
    {
        GoldTmp.text = $"{gold}";
    }

    private void UpdateStoneText(int stone)
    {
        StoneTmp.text = $"{stone}";
    }

    private void UpdateUpgradeCostTexts()
    {
        GameManager gameManager = GameManager.Instance;
        UgCost1Tmp.text = $"{gameManager.GetLowGradeUpgradeCost()}";
        UgCost2Tmp.text = $"{gameManager.GetHeroUpgradeCost()}";
        UgCost3Tmp.text = $"{gameManager.GetHighGradeUpgradeCost()}";

        Ug1LvTmp.text = $"Lv. {gameManager.lowGradeUpgradeLevel+1}";
        Ug2LvTmp.text = $"Lv. {gameManager.heroUpgradeLevel+1}";
        Ug3LvTmp.text = $"Lv. {gameManager.highGradeUpgradeLevel+1}";
        ProbabilityLvTmp.text = $"Lv. {gameManager.SummonLevel}";

        if (ProbabilityLevelUpButton != null)
            ProbabilityLevelUpButton.interactable = !gameManager.IsSummonLevelMax;
    }
}
