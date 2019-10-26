using System.Collections;
using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
public class LaserTower : Tower
{
    public override TowerType TowerType => TowerType.Laser;

    [Header("Laser Tower Specific")]
    [SerializeField]
    float beamRadius = .5f;
    [SerializeField]
    float beamLength = 30f;
    [SerializeField]
    float turnSpeed = 10f;
    [SerializeField]
    Transform beamStart = default;
    [SerializeField]
    Transform turretPivot = default;
    [SerializeField]
    float activeTime = 0.2f;
    bool shooting = false;

    TargetPoint target;
    static Collider[] targetsBuffer = new Collider[1];

    const int enemyLayerMask = 1 << 9;
    static Collider[] buffer = new Collider[20];
    int BufferedCount;

    LineRenderer lineRenderer = default;

    private void Awake()
    {
        base.Init();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public override void GameUpdate()
    {
        if (AcquireFirstTarget(out target))
        {
            Aim();
        }

        if (shooting)
        {
            Shoot();
        }
        else
        {
            if (shootingTimer <= 0f)
            {
                if (target != null)
                {
                    StartCoroutine("StartShooting");
                }
            }
            else
            {
                shootingTimer -= Time.deltaTime * shotsPerSecond;
            }            
        }
        
    }

    IEnumerator StartShooting()
    {
        shooting = true;
        //lineRenderer.enabled = true;

        yield return new WaitForSeconds(activeTime);

        shooting = false;
        shootingTimer = 1f;
        lineRenderer.enabled = false;
        target = null;

        yield break;
    }

    void Aim()
    {
        Vector3 dir = target.Position - turretPivot.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(turretPivot.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        turretPivot.rotation = Quaternion.Euler(0f, rotation.y, 0f);

    }

    void Shoot()
    {

        Vector3 end = beamStart.position;
        end += beamStart.forward.normalized * beamLength;
        lineRenderer.SetPosition(0, beamStart.position);
        lineRenderer.SetPosition(1, end);
        
        //Dibujar linea
        lineRenderer.enabled = true;

        if (FillBuffer(end))
        {
            DamageEnemies();
        }
    }

    bool FillBuffer(Vector3 end)
    {
        BufferedCount = Physics.OverlapCapsuleNonAlloc(
            beamStart.position, end, beamRadius, buffer, enemyLayerMask
        );
        return BufferedCount > 0;
    }

    void DamageEnemies()
    {
        for (int i = 0; i < BufferedCount; i++)
        {
            TargetPoint target = buffer[i].GetComponent<TargetPoint>();
            target.Enemy.ApplyDamage(damage * Time.timeScale);
        }
    }

    public override bool Upgrade()
    {
        bool res = base.Upgrade();

        turret.SetParent(turretPivot);

        return res;
    }

}
