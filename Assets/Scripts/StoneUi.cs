using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoneUi : MonoBehaviour
{
    [SerializeField] private Button rareGachaButton;
    [SerializeField] private Button heroGachaButton;
    [SerializeField] private Button legendGachaButton;
    [SerializeField] private TextMeshProUGUI stoneCountTmp;

    private void OnEnable()
    {
        UpdateStoneText(CurrencyManager.Instance.SummonStone);

        CurrencyManager.Instance.OnSummonStoneChanged += UpdateStoneText;
    }

    private void OnDisable()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnSummonStoneChanged -= UpdateStoneText;
    }

    private void UpdateStoneText(int stone)
    {
        stoneCountTmp.text = $"{stone}";
    }
}