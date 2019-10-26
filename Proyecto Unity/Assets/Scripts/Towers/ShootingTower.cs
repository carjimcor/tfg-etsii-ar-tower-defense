using System.Collections.Generic;
using UnityEngine;

public class ShootingTower : Tower
{
    public override TowerType TowerType => TowerType.Shooting;

    [Header("Shooting Tower Specific")]
    [SerializeField]
    float bulletSpeed = 10f;

    TargetPoint target;
    static Collider[] targetsBuffer = new Collider[1];
    
    public override void GameUpdate()
    {
        if (TrackTarget(ref target) || AcquireTarget(out target))
        {
            if (shootingTimer <= 0f)
            {
                Shoot();
                shootingTimer += 1f;
            }
            shootingTimer -= Time.deltaTime * shotsPerSecond;
        }
    }

    void Shoot()
    {
        Vector3 point = target.Position;
        turret.LookAt(point);

        float d = Vector3.Distance(turret.position, point);
        Game.SpawnBullet().Initialize(target, NextShootingPoint(), bulletSpeed, damage);
    }

}
