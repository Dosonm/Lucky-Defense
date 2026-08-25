using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private GameManager gameManager;
    private CurrencyManager currencyManager;

    [SerializeField] private Monster[] monsters;

    [Header("Wave")]
    [SerializeField] private int waveCount = 10;
    [SerializeField] private int enemiesPerWave = 10;
    [SerializeField] private float waveDuration = 20f;
    [SerializeField] private float spawnInterval = 0.9f;

    [Header("Reward")]
    [SerializeField] private int waveClearBaseGold = 50;
    [SerializeField] private int waveClearGoldPerWave = 10;
    [SerializeField] private int killGold = 2;

    public static int currentWave;

    private float waveEndTime;
    private int lastShownSecond = -1;

    private WaitForSeconds spawnWait;

    [Header("Spawn")]
    [SerializeField] private Vector2 spawnDirection = Vector2.right;

    private void Start()
    {
        gameManager = GameManager.Instance;
        currencyManager = CurrencyManager.Instance;
        spawnWait = new WaitForSeconds(spawnInterval);

        StartCoroutine(SpawnWaves());
    }

    private void Update()
    {
        if (gameManager.IsGameOver || currentWave >= waveCount)
            return;

        int second = Mathf.Max(0, Mathf.CeilToInt(waveEndTime - Time.time));
        if (second == lastShownSecond)
            return;

        lastShownSecond = second;
        gameManager.UpdateTimeUi(second);
    }

    private IEnumerator SpawnWaves()
    {
        for (currentWave = 0; currentWave < waveCount; currentWave++)
        {
            gameManager.UpdateWaveUi();

            waveEndTime = Time.time + waveDuration;

            int spawned = 0;
            while (Time.time < waveEndTime)
            {
                if (gameManager.IsGameOver)
                    yield break;

                if (spawned < enemiesPerWave)
                {
                    SpawnMonster();
                    spawned++;
                }

                yield return spawnWait;
            }

            currencyManager.AddGold(waveClearBaseGold + currentWave * waveClearGoldPerWave);
        }
    }

    private void SpawnMonster()
    {
        if (monsters.Length == 0)
            return;

        Monster prefab = monsters[Random.Range(0, monsters.Length)];

        Monster instance = PoolManager.Instance.Get<Monster>(prefab.gameObject, transform.position, Quaternion.identity);
        instance.Initialize(spawnDirection);

        MonsterTurnDetector turnDetector = instance.GetComponentInChildren<MonsterTurnDetector>();
        if (turnDetector != null)
            turnDetector.ResetCheckpoint();

        MonsterStat stat = instance.GetComponent<MonsterStat>();
        stat.ResetStat();
        stat.onDeath += HandleMonsterDeath;

        gameManager.AddEnemyCount();
    }

    private void HandleMonsterDeath(GameObject monsterObject)
    {
        MonsterStat stat = monsterObject.GetComponent<MonsterStat>();
        stat.onDeath -= HandleMonsterDeath;

        gameManager.RemoveEnemyCount();
        currencyManager.AddGold(killGold);

        PoolManager.Instance.Release(monsterObject);
    }
}