using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween 네임스페이스 추가

public class DamageText : MonoBehaviour
{
    private TMP_Text text;
    
    [Header("Custom Effect Settings")]
    float duration = .8f;    

    private float damage;
    private string innerText;
    private bool isCritical;

    private Sequence currentSequence;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    public void Activate()
    {
        SetText();

        currentSequence?.Kill();
        
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        currentSequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);

        if (isCritical) 
        {
            seq.Append(transform.DOScale(1.5f, duration * 0.1f).SetEase(Ease.OutBack));
            seq.Join(text.DOFade(1f, 0.05f)); 

            seq.AppendInterval(0.1f);

            seq.Append(transform.DOScale(1.0f, duration * 0.3f).SetEase(Ease.OutQuad));
        }
        else 
        {
            seq.Append(transform.DOScale(1.0f, duration * 0.07f).SetEase(Ease.OutBack));
            seq.Join(text.DOFade(1f, 0.05f));

        }
        seq.Join(text.DOFade(0f, duration * 0.3f).SetDelay(.5f)); 


        seq.OnComplete(() =>
        {
            DamageTextManager.instance.ReturnToPool(gameObject, isCritical);
        });
    }

    private void SetText()
    {
        text.text = damage.ToString();
    }

    public void GetInfoDmg(int _damage, bool _isCritical)
    {
        damage = _damage;
        isCritical = _isCritical;
    }

    public void GetInfoText(string _text, bool isTextType)
    {
        innerText = _text;
    }
    private void OnDestroy()
    {
        currentSequence?.Kill();
    }
}