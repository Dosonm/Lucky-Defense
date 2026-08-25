using UnityEngine;
using TMPro;
using DG.Tweening; // DOTween 네임스페이스 추가

public class DamageText : MonoBehaviour
{
    private TMP_Text text;
    
    [Header("Custom Effect Settings")]
    float duration = .8f;     // 전체 연출 지속 시간

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
        
        // 1️⃣ 초기 상태 설정: 완전히 투명하고, 스케일은 0 (팝업 효과 준비)
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0f);
        transform.localScale = Vector3.zero;

        // 2️⃣ DOTween Sequence 생성 (여러 연출을 묶어 관리)
        Sequence seq = DOTween.Sequence();

        currentSequence.SetLink(gameObject, LinkBehaviour.KillOnDisable);

        if (isCritical) // 🌟 크리티컬 데미지 연출 (가장 중요!)
        {
            // [박자감 1] 팍! 하고 크게 나타남 (Ease.OutBack으로 탄성 부여)
            seq.Append(transform.DOScale(1.5f, duration * 0.1f).SetEase(Ease.OutBack));
            seq.Join(text.DOFade(1f, 0.05f)); // 페이드는 아주 빠르게 완료

            // [박자감 2] 살짝 대기 (잠시 눈에 띄게 함)
            seq.AppendInterval(0.1f);

            // [박자감 3] 천천히 작아지며 위로 상승 + 사라짐
            seq.Append(transform.DOScale(1.0f, duration * 0.3f).SetEase(Ease.OutQuad));
        }
        else // 일반 데미지 및 텍스트 연출
        {
            // [박자감 1] 톡! 하고 나타남
            seq.Append(transform.DOScale(1.0f, duration * 0.07f).SetEase(Ease.OutBack));
            seq.Join(text.DOFade(1f, 0.05f));

            // [박자감 2] 바로 위로 상승 + 사라짐
        }
        seq.Join(text.DOFade(0f, duration * 0.3f).SetDelay(.5f)); // 끝부분에서 사라짐


        // 3️⃣ 연출 완료 후 풀에 반환
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
        // 오브젝트가 Destroy될 때 진행 중인 트윈을 강제로 멈춰서 콜백 호출을 막음
        currentSequence?.Kill();
    }
}