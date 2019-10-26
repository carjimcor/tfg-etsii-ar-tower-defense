using UnityEngine;
using System.Collections.Generic;

public class SlowTower : Tower
{
    public override TowerType TowerType => TowerType.Slow;

    static Collider[] targetsBuffer = new Collider[20];

    const int enemyLayerMask = 1 << 9;
    static Collider[] buffer = new Collider[20];
    int BufferedCount;

    [Header("The damage of this tower is the slow percentage applied to enemies")]
    [SerializeField]
    Transform turretRotator = default;

    List<RotateAroundParent> pointRotators = new List<RotateAroundParent>();

    public override void GameUpdate()
    {
        FillBuffer();

        if (BufferedCount > 0)
        {
            Slow();
            EnableRotators(true);
        }
        else
        {
            EnableRotators(false);
        }
    }

    void EnableRotators(bool value)
    {
        foreach (RotateAroundParent rotator in pointRotators)
        {
            rotator.enabled = value;
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

    void Slow()
    {
        for (int i = 0; i < BufferedCount; i++)
        {
            TargetPoint target = buffer[i].GetComponent<TargetPoint>();
            target.Enemy.SlowDown(damage);
        }
    }

    public override bool Upgrade()
    {
        bool res = base.Upgrade();

        turret.SetParent(turretRotator);
        pointRotators.Clear();
        foreach (Transform shootingPoint in shootingPoints)
        {
            pointRotators.Add(shootingPoint.GetComponent<RotateAroundParent>());
        }

        return res;
    }

}
