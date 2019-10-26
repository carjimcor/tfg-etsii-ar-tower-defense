using UnityEngine;
using System.Collections.Generic;

public class MissileTower : Tower
{
    public override TowerType TowerType => TowerType.Missile;

    [Header("Missile Tower Specific")]
    [SerializeField]
    float missileSpeed = 2.5f, blastRadius = 1.5f;

    [SerializeField]
    AnimationCurve blastRadiusCurve = default;

    private void Awake()
    {
        shootingTimer = 1f;
        if (upgrade != null)
            blastRadius = blastRadiusCurve.Evaluate(0);
        base.Init();
    }

    public override void GameUpdate()
    {
        shootingTimer += shotsPerSecond * Time.deltaTime;
        while (shootingTimer >= 1f)
        {
            if (AcquireTarget(out TargetPoint target))
            {
                Launch(target);
                shootingTimer -= 1f;
            }
            else
            {
                shootingTimer = 0.999f;
            }
        }
    }

    public void Launch(TargetPoint target)
    {
        Vector3 positionToLook = target.Position;
        positionToLook.y = turret.position.y;
        turret.LookAt(positionToLook);
        Game.SpawnMissile().Initialize(target, NextShootingPoint(), missileSpeed, blastRadius, damage);
    }

    public override bool Upgrade()
    {
        if (canUpgrade)
        {
            float t = upgradeIndex / (upgrade.turretUpgrades.Length - 1);
            blastRadius = blastRadiusCurve.Evaluate(t);
        }
        return base.Upgrade();
    }
}