using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MythUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameTmp;
    [SerializeField] private Image ProfilImg;

    [SerializeField] private Image NeededUnit1;
    [SerializeField] private Image NeededUnit1Bg;
    [SerializeField] private TextMeshProUGUI NeededUnit1Tmp;

    [SerializeField] private Image NeededUnit2;
    [SerializeField] private Image NeededUnit2Bg;
    [SerializeField] private TextMeshProUGUI NeededUnit2Tmp;

    [SerializeField] private Image NeededUnit3;
    [SerializeField] private Image NeededUnit3Bg;
    [SerializeField] private TextMeshProUGUI NeededUnit3Tmp;

    [SerializeField] private Image NeededUnit4;
    [SerializeField] private Image NeededUnit4Bg;
    [SerializeField] private TextMeshProUGUI NeededUnit4Tmp;

    [SerializeField] private Button CompoundBtn;

    [Header("Myth Prefabs")]
    [SerializeField] private Summons vainPrefab;
    [SerializeField] private Summons ninjaPrefab;

    private Image[] neededUnitImgs;
    private Image[] neededUnitBgs;
    private TextMeshProUGUI[] neededUnitTmps;

    private const string OwnedText = "보유";
    private const string NotOwnedText = "미보유";

    private Summons selectedMyth;

    private static readonly Color GradeLegendColor = ParseColor("F7CD44");
    private static readonly Color GradeHeroColor = ParseColor("E392F9");
    private static readonly Color GradeRareColor = ParseColor("6C9DDF");
    private static readonly Color GradeNormalColor = ParseColor("E6E3D4");

    private static Color ParseColor(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color color);
        return color;
    }

    private static Color GetGradeColor(SummonsGrade grade)
    {
        return grade switch
        {
            SummonsGrade.Legend => GradeLegendColor,
            SummonsGrade.Hero => GradeHeroColor,
            SummonsGrade.Rare => GradeRareColor,
            _ => GradeNormalColor,
        };
    }

    private void Awake()
    {
        neededUnitImgs = new[] { NeededUnit1, NeededUnit2, NeededUnit3, NeededUnit4 };
        neededUnitBgs = new[] { NeededUnit1Bg, NeededUnit2Bg, NeededUnit3Bg, NeededUnit4Bg };
        neededUnitTmps = new[] { NeededUnit1Tmp, NeededUnit2Tmp, NeededUnit3Tmp, NeededUnit4Tmp };
    }

    private void OnEnable()
    {
        SelectNinja();
        UpdateCompoundButton();
    }

    public void SelectVain() => SelectMyth(vainPrefab);
    public void SelectNinja() => SelectMyth(ninjaPrefab);

    private void SelectMyth(Summons mythPrefab)
    {
        if (mythPrefab == null)
            return;

        selectedMyth = mythPrefab;

        if (ProfilImg != null)
            ProfilImg.sprite = mythPrefab.GetComponent<SpriteRenderer>().sprite;

        ChangeName(mythPrefab);
        RefreshNeededUnits();
        UpdateCompoundButton();
    }

    private static SummonsGrade GetGradeOf(SummonsTypes type)
    {
        Summons prefab = GameManager.Instance.GetPrefabForType(type);
        return prefab != null ? prefab.summonsGrade : SummonsGrade.Normal;
    }

    private void ChangeName(Summons mythPrefab)
    {
        NameTmp.text = mythPrefab.name;
    }

    private void RefreshNeededUnits()
    {
        List<SummonsTypes> requiredUnits = new(selectedMyth.RequiredUnits);

        requiredUnits.Sort((a, b) =>
        {
            int gradeCompare = GetGradeOf(b).CompareTo(GetGradeOf(a));
            return gradeCompare != 0 ? gradeCompare : a.CompareTo(b);
        });

        for (int i = 0; i < neededUnitImgs.Length; i++)
        {
            if (i < requiredUnits.Count)
                ShowSlot(i, requiredUnits[i]);
            else
                HideSlot(i);
        }
    }

    private void ShowSlot(int index, SummonsTypes type)
    {
        Summons prefab = GameManager.Instance.GetPrefabForType(type);
        if (prefab == null)
        {
            HideSlot(index);
            return;
        }

        Image img = neededUnitImgs[index];
        Image bg = neededUnitBgs[index];
        TextMeshProUGUI tmp = neededUnitTmps[index];

        if (img != null)
        {
            img.sprite = prefab.GetComponent<SpriteRenderer>().sprite;
            SetImageVisible(img, true);
        }

        if (bg != null)
        {
            bg.color = GetGradeColor(prefab.summonsGrade);
            SetImageVisible(bg, true);
        }

        if (tmp != null)
        {
            bool owned = GridManager.Instance.HasUnitOfType(type);
            tmp.text = owned ? OwnedText : NotOwnedText;
            tmp.gameObject.SetActive(true);
        }
    }

    private void HideSlot(int index)
    {
        SetImageVisible(neededUnitImgs[index], false);
        SetImageVisible(neededUnitBgs[index], false);

        TextMeshProUGUI tmp = neededUnitTmps[index];
        if (tmp != null)
            tmp.gameObject.SetActive(false);
    }

    private static void SetImageVisible(Image img, bool visible)
    {
        if (img == null)
            return;

        Color color = img.color;
        color.a = visible ? 1f : 0f;
        img.color = color;
        img.raycastTarget = visible;
    }

    private void UpdateCompoundButton()
    {
        if (CompoundBtn == null || selectedMyth == null)
            return;

        CompoundBtn.interactable = GridManager.Instance.CanCompoundMyth(selectedMyth.RequiredUnits);
    }

    public void OnCompoundBtnClicked()
    {
        if (selectedMyth == null)
            return;

        GridManager.Instance.CompoundMyth(selectedMyth);
        RefreshNeededUnits();
        UpdateCompoundButton();
    }
}