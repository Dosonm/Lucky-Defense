using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance; // 싱글톤 인스턴스
    public Canvas canvas;

    [Header("Prefabs")]
    [SerializeField] GameObject damageTextPrefab; // 일반 데미지 텍스트
    [SerializeField] GameObject damageTextPrefabCrit; // 크리티컬 데미지 텍스트

    [Header("Settings")]
    float textSpacing = .1f; // 텍스트 간 간격
    
    private List<GameObject> activeDamageTexts = new List<GameObject>(); 

    [Header("Object Pool")]
    [SerializeField] int poolSize = 100;
    private Queue<GameObject> textPool = new Queue<GameObject>();
    private Queue<GameObject> CritTextPool = new Queue<GameObject>();

    private void Awake()
    {
        // 최대 트윈 1000개, 시퀀스 100개까지 수용 가능하도록 설정
        DOTween.SetTweensCapacity(1000, 100);
        
        // 가비지 컬렉션 최적화를 위해 트윈 재사용 설정
        DOTween.defaultRecyclable = true;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        InitializePool();
    }

    #region Pooling System
    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObjectInPool(damageTextPrefab, textPool);
            CreateNewObjectInPool(damageTextPrefabCrit, CritTextPool);
        }
    }

    private void CreateNewObjectInPool(GameObject prefab, Queue<GameObject> pool)
    {
        if (prefab == null) return;
        GameObject obj = Instantiate(prefab, canvas.transform);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }

    private GameObject GetFromPool(bool isCritical)
    {
        Queue<GameObject> targetPool = isCritical ? CritTextPool : textPool;
        GameObject prefab = isCritical ? damageTextPrefabCrit : damageTextPrefab;

        GameObject obj;
        if (targetPool.Count > 0)
        {
            obj = targetPool.Dequeue();
        }
        else
        {
            obj = Instantiate(prefab, canvas.transform);
        }

        obj.SetActive(true);
        return obj;
    }

    public void ReturnToPool(GameObject obj, bool _isCritical)
    {
        obj.SetActive(false);
        if (_isCritical) CritTextPool.Enqueue(obj);
        else textPool.Enqueue(obj);
    }
    #endregion

    /// <summary>
    /// 실제 데미지 텍스트를 생성하는 함수
    /// </summary>
    public void CreateDamageText(Vector2 position, int damage, bool isCritical)
    {
        MoveExistingTextsUp();

        GameObject newText = GetFromPool(isCritical);
        
        newText.transform.position = position;

        DamageText dmgScript = newText.GetComponent<DamageText>();
        if (dmgScript != null)
        {
            dmgScript.GetInfoDmg(damage, isCritical);
            activeDamageTexts.Add(newText);
            dmgScript.Activate(); // 여기서 DOTween Animation 실행
        }
    }
    private void MoveExistingTextsUp()
    {
        for (int i = activeDamageTexts.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeDamageTexts[i];

            if (obj == null || !obj.activeInHierarchy)
            {
                activeDamageTexts.RemoveAt(i);
                continue;
            }

            obj.transform.DOKill(); 
            obj.transform.DOMoveY(obj.transform.position.y + textSpacing, 0.1f).SetEase(Ease.OutQuad);
        }
    }

    /// <summary>
    /// 일반 텍스트 생성 (풀링 미적용 버전 - 필요시 수정 가능)
    /// </summary>
    /* public void creatText(Vector2 position, string text)
    {
        GameObject newText = Instantiate(damageTextPrefab, canvas.transform);
        newText.transform.position = position;

        DamageText dmgScript = newText.GetComponent<DamageText>();
        dmgScript.GetInfoText(text, true);
        activeDamageTexts.Add(newText);
        dmgScript.Activate();
    }

    public void RemoveText(GameObject text)
    {
        if (activeDamageTexts.Contains(text))
        {
            activeDamageTexts.Remove(text);
        }
    } */
}