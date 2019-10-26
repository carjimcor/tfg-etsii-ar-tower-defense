using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory
{

    [SerializeField]
    Missile missilePrefab = default;

    [SerializeField]
    Bullet bulletPrefab = default;

    [SerializeField]
    Explosion explosionPrefab = default;

    public Missile Missile => Get(missilePrefab);
    public Bullet Bullet => Get(bulletPrefab);

    public Explosion Explosion => Get(explosionPrefab);

    T Get<T>(T prefab) where T : WarEntity
    {
        T instance = CreateGameObjectInstance(prefab);
        instance.OriginFactory = this;
        return instance;
    }

    public void Reclaim(WarEntity entity)
    {
        Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");
        Destroy(entity.gameObject);
    }
}