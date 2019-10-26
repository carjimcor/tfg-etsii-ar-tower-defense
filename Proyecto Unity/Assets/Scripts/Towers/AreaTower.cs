using UnityEngine;

public class AreaTower : Tower
{
    public override TowerType TowerType => TowerType.Area;

    static Collider[] targetsBuffer = new Collider[20];

    const int enemyLayerMask = 1 << 9;
    static Collider[] buffer = new Collider[20];
    int BufferedCount;

    [SerializeField]
    Rotator turretRotator = default;

    public override void GameUpdate()
    {
        FillBuffer();

        if (shootingTimer <= 0f)
        {            
            if (BufferedCount > 0)
            {
                Shoot();
            }
            shootingTimer += 1f;
        }
        shootingTimer -= Time.deltaTime * shotsPerSecond;

        if (BufferedCount > 0)
        {
            if (!turretRotator.enabled)
            {
                turretRotator.enabled = true;
            }
        }
        else
        {
            if (turretRotator.enabled)
            {
                turretRotator.enabled = false;
            }
        }

    }

    bool FillBuffer()
    {
        Vector3 top = transform.position;
        top.y += 3f;
        BufferedCount = Physics.OverlapCapsuleNonAlloc(
            transform.position, top, targetingRange, buffer, enemyLayerMask
        );
        return BufferedCount > 0;
    }

    void Shoot()
    {
        for (int i = 0; i < BufferedCount; i++)
        {
            TargetPoint target = buffer[i].GetComponent<TargetPoint>();
            target.Enemy.ApplyDamage(damage);
            target.Enemy.SlowDown(.1f);
        }
    }

    void Slow()
    {
        for (int i = 0; i < BufferedCount; i++)
        {
            TargetPoint target = buffer[i].GetComponent<TargetPoint>();
            target.Enemy.SlowDown(1f);
        }
    }

    public override bool Upgrade()
    {
        bool res = base.Upgrade();

        turret.SetParent(turretRotator.transform);

        return res;
    }

}
