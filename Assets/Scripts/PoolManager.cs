using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    // 반환 시 어떤 prefab 풀로 되돌려야 하는지 알기 위한 역참조
    private readonly Dictionary<GameObject, GameObject> instanceToPrefab = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }

        GameObject instance;
        if (queue.Count > 0)
        {
            instance = queue.Dequeue();
        }
        else
        {
            instance = Instantiate(prefab, transform);
            instanceToPrefab[instance] = prefab;
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    //Get함수의 기능 확장
    public T Get<T>(GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return Get(prefab, position, rotation).GetComponent<T>();
    }

    public void Release(GameObject instance)
    {
        if (!instanceToPrefab.TryGetValue(instance, out GameObject prefab))
        {
            Debug.LogWarning($"{instance.name}은(는) PoolManager로 생성된 오브젝트가 아닙니다.");
            Destroy(instance);
            return;
        }

        instance.SetActive(false);
        pools[prefab].Enqueue(instance);
    }
}