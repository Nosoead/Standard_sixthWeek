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
    COUNT // COUNT는 실제 쓰이는 enum이 아니라 몇 개가 들어있는지에 대한 값임
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

        // 생성위치를 하나하나 다 등록해놓고 쓰는 것보다 같은 부모 아래에 두고 부모하나만 등록해서 쓰려고 하는거에요!
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
        // StartNextWave()처럼 작성한다고 오류가 발생하진 않아요!
        // 더 무서운 코루틴이 정상적으로 작동하지 않는 오류가 발생합니다. 꼭 조심해주세요!
        StartCoroutine(StartNextWave());
    }

    // 코루틴으로 작성되었다는 것은 실행을 여러 프레임에 걸쳐서 할 것이라는 뜻이에요!
    // 몬스터가 한 프레임에서 왕창 생성되고 끝나면 안되겠죠? 시간이 지남에 따라 천천히 나와야 되니까요!
    IEnumerator StartNextWave()
    {
        while (true)
        {
            if (currentSpawnCount == 0)
            {
                UpdateWaveUI();
                // new WaitForSeconds를 GC를 피하게 하기 위해 캐싱하기도 합니다.
                yield return new WaitForSeconds(2f);

                ProcessWaveConditions();

                // yield return Coroutine으로 하위 코루틴이 끝날 때까지 기다릴 수 있어요.
                // 중첩 코루틴(Nested Coroutine)이라고 합니다.
                yield return StartCoroutine(SpawnEnemiesInWave());

                currentWaveIndex++;
            }

            // yield return null은 1프레임 뒤라는 의미에요!
            yield return null;
        }
    }

    void ProcessWaveConditions()
    {
        // % 는 나머지 연산자죠?
        // 나머지 값에 따라 조건문을 주어서, 주기성이 있는 대상에 활용하기도 해요.

        // 20 스테이지마다 이벤트가 발생해요.
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
        // 생성한 적에 OnEnemyDeath를 등록해요.
        enemy.GetComponent<HealthSystem>().OnDeath += OnEnemyDeath;
        currentSpawnCount++;
    }

    // 생성될 수 있는 곳이 늘어나는 로직, 최댓값을 넘지 않아요.
    void IncreaseSpawnPositions()
    {
        // 삼항연산자 기억하시죠? (조건 ? 조건이 참일 때 : 조건이 거짓일 때)처럼 구문이 작성돼요!
        waveSpawnPosCount = waveSpawnPosCount + 1 > spawnPositions.Count ? waveSpawnPosCount : waveSpawnPosCount + 1;
        waveSpawnCount = 0;
    }

    void IncreaseWaveSpawnCount()
    {
        waveSpawnCount += 1;
    }


    private void UpgradeStatInit()
    {
        Debug.Log("UpgradeStatInit 호출");
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
        //UI 켜주면 됨
        gameOverUI.SetActive(true);
        StopAllCoroutines();
    }

    private void UpdateHealthUI()
    {
        hpGaugeSlider.value = playerHealthSystem.CurrentHealth / playerHealthSystem.MaxHealth;
    }

    private void UpdateWaveUI()
    {
        // 1웨이브부터 나올 수 있도록 1을 더해줘요.
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
