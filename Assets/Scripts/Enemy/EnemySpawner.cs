using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private WaveSO[] waveList;

    private List<EnemySO> probList = new List<EnemySO>();
    private float maxGameBarTimer = 0;
    private float timerGameBar = 0;

    private float spawnTimer = 0;
    private float spawnPeriod = 0;

    private float currentTimer = 0;

    private int enemyCount;
    private bool waitWave;
    private bool activeWave;
    private bool canSpawnEnemies;
    private int currentWave = 0;
    private bool hasEnded = false;

    [Header("Events")]
    public UnityEvent<float> OnGameBarUpdated = new UnityEvent<float>();
    public UnityEvent<float> OnNewWaveAdded = new UnityEvent<float>();
    public UnityEvent OnWaveStart = new UnityEvent();
    public UnityEvent OnIncomingWave = new UnityEvent();
    public EnemyInvokeChannel invokeEnemyChannel;
    public BoolChannelSO OnWaveFinishChannel;

    public void StartEnemySpawner()
    {
        hasEnded = false;
        spawnPeriod = waveList[currentWave].newSpawnTime;

        foreach (var wave in waveList)
        {
            float waveTimerBeforeWave = wave.newSpawnTime * wave.totalEnemyBeforeWave + wave.delayBeforeWave +
                                        wave.TimeBeforeNewPreWaveStart + wave.totalEnemyWave * wave.SpawnTimeInWave;

            maxGameBarTimer += waveTimerBeforeWave;
            Debug.Log(maxGameBarTimer);
        }

        currentTimer = timerGameBar / maxGameBarTimer;
        OnGameBarUpdated.Invoke(currentTimer);

        float waveTimer = 0;
        foreach (var wave in waveList)
        {
            float imageNormalizePosition =
                (waveTimer + wave.newSpawnTime * wave.totalEnemyBeforeWave + wave.delayBeforeWave +
                 wave.TimeBeforeNewPreWaveStart) / maxGameBarTimer;

            waveTimer += wave.newSpawnTime * wave.totalEnemyBeforeWave + wave.delayBeforeWave +
                         wave.TimeBeforeNewPreWaveStart + wave.totalEnemyWave * wave.SpawnTimeInWave;
            Debug.Log(waveTimer);
            OnNewWaveAdded.Invoke(imageNormalizePosition);
        }

        waitWave = true;
    }

    private void Update()
    {
        timerGameBar += Time.deltaTime;
        currentTimer = timerGameBar / maxGameBarTimer;
        OnGameBarUpdated.Invoke(Mathf.Clamp01(currentTimer));

        //Check if all wave have finish
        if (currentWave < waveList.Length)
        {
            //check Wave
            if (waitWave)
            {
                StartCoroutine(BeforeWave());
            }
            else if (!activeWave && enemyCount >= waveList[currentWave].totalEnemyBeforeWave)
            {
                StartCoroutine(EnableWave());
            }
            else if (activeWave && enemyCount >= waveList[currentWave].totalEnemyWave)
            {
                StartCoroutine(SetNextWave());
            }

            if (spawnTimer > spawnPeriod)
            {
                SpawnNewEnemy();
            }

            if (!waitWave)
            {
                spawnTimer += Time.deltaTime;
            }
        }
        else if (!hasEnded)
        {
            hasEnded = true;
            OnWaveFinishChannel.RaiseEvent(hasEnded);
        }
    }

    private void SpawnNewEnemy()
    {
        if (canSpawnEnemies)
        {
            spawnTimer = 0;

            SortEnemies();

            invokeEnemyChannel.RaiseEvent(probList);
            enemyCount++;
        }
    }

    private IEnumerator BeforeWave()
    {
        yield return new WaitForSeconds(waveList[currentWave].TimeBeforeNewPreWaveStart);
        waitWave = false;
        canSpawnEnemies = true;
        yield return null;
    }

    private IEnumerator EnableWave()
    {
        canSpawnEnemies = false;
        enemyCount = 0;

        yield return new WaitForSeconds(waveList[currentWave].delayBeforeWave);

        Debug.Log("EnableWave");
        activeWave = true;
        canSpawnEnemies = true;
        spawnPeriod = waveList[currentWave].SpawnTimeInWave;
        OnIncomingWave.Invoke();

        yield return null;
    }

    private IEnumerator SetNextWave()
    {
        enemyCount = 0;

        currentWave++;

        if (currentWave < waveList.Length)
        {
            spawnPeriod = waveList[currentWave].newSpawnTime;
        }

        OnWaveStart.Invoke();

        activeWave = false;
        canSpawnEnemies = false;
        waitWave = true;
        Debug.Log("SetNextWave");

        yield return null;
    }


    private void SortEnemies()
    {
        if (currentWave < waveList.Length)
        {
            foreach (EnemyTypeProb newEnemyType in waveList[currentWave].enemyTypes)
            {
                for (int i = 0; i < newEnemyType.probability; i++)
                {
                    probList.Add(newEnemyType.Type);
                }
            }
        }
    }
}