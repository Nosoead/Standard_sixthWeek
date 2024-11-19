using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum UpgradeOption
{
    MaxHealth,
    AttackPower,
    Speed,
    Knockback,
    AttackDelay,
    NumberOfProjectiles,
    COUNT // COUNT�� ���� ���̴� enum�� �ƴ϶� �� ���� ����ִ����� ���� ����
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private string playerTag;

    [SerializeField] private CharacterStat defaultStat;
    [SerializeField] private CharacterStat rangedStat;

    public ObjectPool ObjectPool { get; private set; }
    public Transform Player { get; private set; }
    public ParticleSystem EffectParticle;

    private HealthSystem playerHealthSystem;

    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private Slider hpGaugeSlider;
    [SerializeField] private GameObject gameOverUI;

    [SerializeField] private int currentWaveIndex = 0;
    private int currentSpawnCount = 0;
    private int waveSpawnCount = 0;
    private int waveSpawnPosCount = 0;

    public float spawnInterval = .5f;
    public List<GameObject> enemyPrefebs = new List<GameObject>();

    [SerializeField] private Transform spawnPositionsRoot;
    private List<Transform> spawnPositions = new List<Transform>();

    [SerializeField] private List<GameObject> Rewards = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        Player = GameObject.FindGameObjectWithTag(playerTag).transform;
        EffectParticle = GameObject.FindGameObjectWithTag("Particle").GetComponent<ParticleSystem>();

        ObjectPool = GetComponent<ObjectPool>();

        playerHealthSystem = Player.GetComponent<HealthSystem>();
        playerHealthSystem.OnDamage += UpdateHealthUI;
        playerHealthSystem.OnHeal += UpdateHealthUI;
        playerHealthSystem.OnDeath += GameOver;

        UpgradeStatInitInit();

        // ������ġ�� �ϳ��ϳ� �� ����س��� ���� �ͺ��� ���� �θ� �Ʒ��� �ΰ� �θ��ϳ��� ����ؼ� ������ �ϴ°ſ���!
        for (int i = 0; i < spawnPositionsRoot.childCount; i++)
        {
            spawnPositions.Add(spawnPositionsRoot.GetChild(i));
        }
    }

    private void UpgradeStatInitInit()
    {
        defaultStat.statsChangeType = StatsChangeType.Add;
        defaultStat.attackSO = Instantiate(defaultStat.attackSO);

        rangedStat.statsChangeType = StatsChangeType.Add;
        rangedStat.attackSO = Instantiate(rangedStat.attackSO);
    }

    private void Start()
    {
        UpgradeStatInit();
        // StartNextWave()ó�� �ۼ��Ѵٰ� ������ �߻����� �ʾƿ�!
        // �� ������ �ڷ�ƾ�� ���������� �۵����� �ʴ� ������ �߻��մϴ�. �� �������ּ���!
        StartCoroutine(StartNextWave());
    }

    // �ڷ�ƾ���� �ۼ��Ǿ��ٴ� ���� ������ ���� �����ӿ� ���ļ� �� ���̶�� ���̿���!
    // ���Ͱ� �� �����ӿ��� ��â �����ǰ� ������ �ȵǰ���? �ð��� ������ ���� õõ�� ���;� �Ǵϱ��!
    IEnumerator StartNextWave()
    {
        while (true)
        {
            if (currentSpawnCount == 0)
            {
                UpdateWaveUI();
                // new WaitForSeconds�� GC�� ���ϰ� �ϱ� ���� ĳ���ϱ⵵ �մϴ�.
                yield return new WaitForSeconds(2f);

                ProcessWaveConditions();

                // yield return Coroutine���� ���� �ڷ�ƾ�� ���� ������ ��ٸ� �� �־��.
                // ��ø �ڷ�ƾ(Nested Coroutine)�̶�� �մϴ�.
                yield return StartCoroutine(SpawnEnemiesInWave());

                currentWaveIndex++;
            }

            // yield return null�� 1������ �ڶ�� �ǹ̿���!
            yield return null;
        }
    }

    void ProcessWaveConditions()
    {
        // % �� ������ ��������?
        // ������ ���� ���� ���ǹ��� �־, �ֱ⼺�� �ִ� ��� Ȱ���ϱ⵵ �ؿ�.

        // 20 ������������ �̺�Ʈ�� �߻��ؿ�.
        if (currentWaveIndex % 20 == 0)
        {
            RandomUpgrade();
        }

        if (currentWaveIndex % 10 == 0)
        {
            IncreaseSpawnPositions();
        }

        if (currentWaveIndex % 5 == 0)
        {
            CreateReward();
        }

        if (currentWaveIndex % 3 == 0)
        {
            IncreaseWaveSpawnCount();
        }
    }

    IEnumerator SpawnEnemiesInWave()
    {
        for (int i = 0; i < waveSpawnPosCount; i++)
        {
            int posIdx = UnityEngine.Random.Range(0, spawnPositions.Count);
            for (int j = 0; j < waveSpawnCount; j++)
            {
                SpawnEnemyAtPosition(posIdx);
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }

    void SpawnEnemyAtPosition(int posIdx)
    {
        int prefabIdx = UnityEngine.Random.Range(0, enemyPrefebs.Count);
        GameObject enemy = Instantiate(enemyPrefebs[prefabIdx], spawnPositions[posIdx].position, Quaternion.identity);
        enemy.GetComponent<CharacterStatsHandler>().AddStatModifier(defaultStat);
        enemy.GetComponent<CharacterStatsHandler>().AddStatModifier(rangedStat);
        // ������ ���� OnEnemyDeath�� ����ؿ�.
        enemy.GetComponent<HealthSystem>().OnDeath += OnEnemyDeath;
        currentSpawnCount++;
    }

    // ������ �� �ִ� ���� �þ�� ����, �ִ��� ���� �ʾƿ�.
    void IncreaseSpawnPositions()
    {
        // ���׿����� ����Ͻ���? (���� ? ������ ���� �� : ������ ������ ��)ó�� ������ �ۼ��ſ�!
        waveSpawnPosCount = waveSpawnPosCount + 1 > spawnPositions.Count ? waveSpawnPosCount : waveSpawnPosCount + 1;
        waveSpawnCount = 0;
    }

    void IncreaseWaveSpawnCount()
    {
        waveSpawnCount += 1;
    }


    private void UpgradeStatInit()
    {
        Debug.Log("UpgradeStatInit ȣ��");
    }

    private void RandomUpgrade()
    {
        UpgradeOption option = (UpgradeOption)UnityEngine.Random.Range(0, (int)UpgradeOption.COUNT);
        switch (option)
        {
            case UpgradeOption.MaxHealth:
                defaultStat.maxHealth += 2;
                break;

            case UpgradeOption.AttackPower:
                defaultStat.attackSO.power += 1;
                break;

            case UpgradeOption.Speed:
                defaultStat.speed += 0.1f;
                break;

            case UpgradeOption.Knockback:
                defaultStat.attackSO.isOnKnockBack = true;
                defaultStat.attackSO.knockBackPower += 1;
                defaultStat.attackSO.knockBackTime = 0.1f;
                break;

            case UpgradeOption.AttackDelay:
                defaultStat.attackSO.delay -= 0.05f;
                break;

            case UpgradeOption.NumberOfProjectiles:
                RangedAttackSO rangedAttackData = rangedStat.attackSO as RangedAttackSO;
                if (rangedAttackData != null) rangedAttackData.numberOfProjectilesPerShot += 1;
                break;

            default:
                break;
        }

    }

    private void CreateReward()
    {
        int selectedRewardIndex = UnityEngine.Random.Range(0, Rewards.Count);
        int randomPOsitionIndex = UnityEngine.Random.Range(0, spawnPositions.Count);

        GameObject obj = Rewards[selectedRewardIndex];
        Instantiate(obj, spawnPositions[randomPOsitionIndex].position, Quaternion.identity);
    }

    private void OnEnemyDeath()
    {
        currentSpawnCount--;
    }

    private void GameOver()
    {
        //UI ���ָ� ��
        gameOverUI.SetActive(true);
        StopAllCoroutines();
    }

    private void UpdateHealthUI()
    {
        hpGaugeSlider.value = playerHealthSystem.CurrentHealth / playerHealthSystem.MaxHealth;
    }

    private void UpdateWaveUI()
    {
        // 1���̺���� ���� �� �ֵ��� 1�� �������.
        waveText.text = (currentWaveIndex + 1).ToString();
        //waveText.text = $"{currentWaveIndex + 1}";
    }


    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
