using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Game))]
public class WaveSpawner : MonoBehaviour
{
    private Game game;

    [SerializeField]
    bool endless = false;
    [SerializeField]
    bool random = false;
    [SerializeField]
    int enemyCount = 5;
    [HideInInspector]
    public int currentWave = 1;
    [SerializeField]
    Text waveText = default;

    public enum SpawnState { SPAWNING, WAITING, COUNTING }

    public float timeBetweenWaves = 5f;
    [HideInInspector]
    public float waveCountDown;

    public Wave[] waves;
    private int nextWaveIndex = 0;

    private SpawnState state = SpawnState.COUNTING;

    void Awake()
    {
        game = GetComponent<Game>();
        UpdateWaveText();
    }

    void Start()
    {
        waveCountDown = timeBetweenWaves;
    }

    void Update()
    {
        if (state == SpawnState.WAITING)
        {
            if (game.EnemiesCount <= 0)
            {
                WaveCompleted();
            }
            else
            {
                return;
            }
        }

        if (waveCountDown <= 0f)
        {
            if (state != SpawnState.SPAWNING)
            {
                StartCoroutine(SpawnWave(waves[nextWaveIndex]));
            }
        }
        else
        {
            waveCountDown -= Time.deltaTime;
        }

    }

    void UpdateWaveText()
    {
        if (endless)
        {
            waveText.text = currentWave.ToString();
        }
        else
        {
            waveText.text = currentWave + "/" + waves.Length;
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        state = SpawnState.SPAWNING;

        for (int sliceIndex = 0; sliceIndex < wave.waveSlices.Length; sliceIndex++)
        {
            Vector3Int waveSlice = wave.waveSlices[sliceIndex];

            int spawnIdx = waveSlice.x;
            int enemyIdx = waveSlice.y;

            for (int i = 0; i < waveSlice.z; i++)
            {
                Game.SpawnEnemy(spawnIdx, enemyIdx);
                if (i < waveSlice.z - 1)
                {
                    yield return new WaitForSeconds(wave.delayBetweenEnemies);
                }
            }

            if (sliceIndex < wave.waveSlices.Length - 1)
            {
                yield return new WaitForSeconds(wave.delayBetweenSlices);
            }

        }

        state = SpawnState.WAITING;

        yield break;
    }

    void WaveCompleted()
    {
        state = SpawnState.COUNTING;
        waveCountDown = timeBetweenWaves;

        nextWaveIndex++;
        currentWave++;
        UpdateWaveText();

        if (nextWaveIndex >= waves.Length)
        {
            if (endless)
            {
                Game.UpdateEnemyHealth(currentWave);

                if (random)
                {
                    if (currentWave % 3 == 0)
                        enemyCount++;

                    nextWaveIndex--;
                    Wave previousWave = waves[nextWaveIndex];

                    Wave newWave = Wave.RandomWave(enemyCount);
                    newWave.delayBetweenEnemies = previousWave.delayBetweenEnemies;
                    newWave.delayBetweenSlices = previousWave.delayBetweenSlices;

                    waves[nextWaveIndex] = newWave;
                }
                else
                {
                    nextWaveIndex = 0;
                }                
            }
            else
            {
                Game.LevelWon();
            }
        }
    }
}
