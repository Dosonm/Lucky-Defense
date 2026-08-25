using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopupEffect : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text text;

    [Header("Animation")]
    [SerializeField] private float moveDistance = 40f;
    [SerializeField] private float duration = 0.8f;

    private Sequence sequence;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
    }

    public void Play(string message, Sprite sprite)
    {
        sequence?.Kill();

        icon.sprite = sprite;
        text.text = message;

        rectTransform.anchoredPosition = Vector2.zero;
        text.alpha = 1f;
        icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1f);

        sequence = DOTween.Sequence();
        sequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);

        sequence.Append(rectTransform.DOAnchorPosY(moveDistance, duration).SetEase(Ease.OutQuad));
        sequence.Join(text.DOFade(0f, duration).SetEase(Ease.InQuad));
        sequence.Join(icon.DOFade(0f, duration).SetEase(Ease.InQuad));

        sequence.OnComplete(() => PoolManager.Instance.Release(gameObject));
    }

    private void OnDisable()
    {
        sequence?.Kill();
    }
}