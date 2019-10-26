using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Wave
{
    [Header("SpawnPoint / EnemyType / Amount")]
    public Vector3Int[] waveSlices;
    public float delayBetweenEnemies = 1f;
    public float delayBetweenSlices = 1f;

    public static Wave RandomWave(int totalEnemies)
    {
        Wave res = new Wave();
        List<Vector3Int> tempSlices = new List<Vector3Int>();

        while (totalEnemies > 0)
        {
            int spawnPoint = Random.Range(0, GameBoard.SpawnCount());
            int enemyType = Random.Range(0, Game.enemyTypesCount());
            int amount = Random.Range(1, Mathf.Min(totalEnemies, 6));

            Vector3Int newSlice = new Vector3Int(spawnPoint, enemyType, amount);
            tempSlices.Add(newSlice);

            totalEnemies -= amount;
        }

        res.waveSlices = new Vector3Int[tempSlices.Count];

        for (int i = 0; i < res.waveSlices.Length; i++)
        {
            res.waveSlices[i] = tempSlices[i];
        }

        return res;
    }

    //public static Wave[] RandomWaves(int minEnemies, int maxEnemies)
    //{

    //}
}