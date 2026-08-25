using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance; 
    public Canvas canvas;

    [Header("Prefabs")]
    [SerializeField] GameObject damageTextPrefab; 
    [SerializeField] GameObject damageTextPrefabCrit; 

    [Header("Settings")]
    float textSpacing = .1f; 
    
    private List<GameObject> activeDamageTexts = new List<GameObject>(); 

    [Header("Object Pool")]
    [SerializeField] int poolSize = 100;
    private Queue<GameObject> textPool = new Queue<GameObject>();
    private Queue<GameObject> CritTextPool = new Queue<GameObject>();

    private void Awake()
    {
        DOTween.SetTweensCapacity(1000, 100);
        
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
            dmgScript.Activate(); 
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
}