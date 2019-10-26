using UnityEngine;
using System.Collections.Generic;

public abstract class Tower : GameTileContent
{
    #region Variables
    protected float shootingTimer = 0f;
    protected int upgradeIndex = 0;
    int nextShootingPoint = 0;
    protected List<Transform> shootingPoints;

    // Inspector Variables
    public abstract TowerType TowerType { get; }
    [SerializeField]
    protected Transform turret = default;
    [HideInInspector]
    public int upgradeCost = 10, sellCost = 5;
    [SerializeField]
    protected Upgrade upgrade = default;
    [HideInInspector]
    public bool canUpgrade = false;

    [Header("Attributes")]
    [SerializeField]
    protected float targetingRange = 3.5f, damage = 5f, shotsPerSecond = 1f;
    public float Range => targetingRange;
    public int currentCost = 5;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        if (upgrade != null)
        {
            canUpgrade = true;
            damage = upgrade.damage.Evaluate(0f);
            targetingRange = upgrade.range.Evaluate(0f);
            shotsPerSecond = upgrade.shotsPerSecond.Evaluate(0f);
            upgradeCost = Mathf.CeilToInt(currentCost * upgrade.upgradePercentage);
            sellCost = Mathf.CeilToInt(currentCost * upgrade.sellPercentage);

            // The first turret Transform is ignored
            upgradeIndex++;
        }
        shootingPoints = new List<Transform>();
        RestartShootingPoints();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 position = transform.localPosition;
        position.y += 0.01f;
        Gizmos.DrawWireSphere(position, targetingRange);
    }
    #endregion

    #region Shooting Methods
    protected bool AcquireTarget(out TargetPoint target)
    {
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            target = TargetPoint.RandomBuffered;
            return true;
        }
        target = null;
        return false;
    }

    protected bool AcquireFirstTarget(out TargetPoint target)
    {
        if (TargetPoint.FillBuffer(transform.localPosition, targetingRange))
        {
            target = TargetPoint.FirstBuffered;
            return true;
        }
        target = null;
        return false;
    }

    protected bool TrackTarget(ref TargetPoint target)
    {
        if (target == null)
        {
            return false;
        }
        Vector3 a = transform.localPosition;
        Vector3 b = target.Position;
        float x = a.x - b.x;
        float z = a.z - b.z;
        float r = targetingRange + 0.125f;
        if (x * x + z * z > r * r)
        {
            target = null;
            return false;
        }
        return true;
    }

    public virtual void RestartShootingPoints()
    {
        shootingPoints.Clear();
        for (int i = 0; i < turret.childCount; i++)
            shootingPoints.Add(turret.GetChild(i));
        nextShootingPoint = 0;
    }

    public virtual Transform NextShootingPoint()
    {
        Debug.Assert(shootingPoints.Count > 0, "No shooting Points!");
        Transform res = shootingPoints[nextShootingPoint];
        nextShootingPoint++;
        if (nextShootingPoint >= shootingPoints.Count)
            nextShootingPoint = 0;
        return res;
    }
    #endregion

    #region Tower Modification
    public virtual bool Upgrade()
    {
        if (canUpgrade)
        {
            // Visual enhancement
            Vector3 position = turret.position;
            Quaternion rotation = turret.rotation;
            Destroy(turret.gameObject);

            turret = Instantiate(upgrade.turretUpgrades[upgradeIndex], transform);
            turret.position = position;
            turret.rotation = rotation;

            // Attribute changing
            float t = 1f * upgradeIndex / (upgrade.turretUpgrades.Length - 1);
            damage = upgrade.damage.Evaluate(t);
            targetingRange = upgrade.range.Evaluate(t);
            shotsPerSecond = upgrade.shotsPerSecond.Evaluate(t);

            // Cost
            currentCost = upgradeCost;
            upgradeCost = Mathf.CeilToInt(currentCost * upgrade.upgradePercentage);
            sellCost = Mathf.CeilToInt(currentCost * upgrade.sellPercentage);

            // ShootingPoints
            RestartShootingPoints();

            // Check if this is the last ugrade
            upgradeIndex++;
            if (upgradeIndex >= upgrade.turretUpgrades.Length)
                canUpgrade = false;
            return true;
        }

        canUpgrade = false;
        return false;
    }

    public void Sell()
    {
        GameBoard.RemoveUpdatingContent(this);
    }
    #endregion
}
