using UnityEngine;

[CreateAssetMenu]
public class EnemyFactory : GameObjectFactory
{

    [SerializeField]
    Enemy[] prefabs = default;

    [HideInInspector]
    public Game game;

    public int PrefabCount => prefabs.Length;

    public Enemy Get(int prefabIndex)
    {
        Enemy instance;
        if (prefabIndex >= 0 && prefabIndex < PrefabCount)
        {
            instance = CreateGameObjectInstance(prefabs[prefabIndex]);
        }
        else
        {
            instance = CreateGameObjectInstance(prefabs[Random.Range(0, PrefabCount)]);
        }
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(Enemy enemy)
    {
        Debug.Assert(enemy.OriginFactory == this, "Wrong factory reclaimed!");
                
        Destroy(enemy.gameObject);
    }
}