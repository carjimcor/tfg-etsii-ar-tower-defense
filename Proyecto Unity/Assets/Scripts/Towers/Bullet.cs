using UnityEngine;

public class Bullet : WarEntity
{
    float age;

    TargetPoint target = default;

    [SerializeField]
    GameObject impactEffect = default;

    float speed, damage;

    public void Initialize(
        TargetPoint target, Transform shootingPoint,
        float speed, float damage
    )
    {
        this.target = target;
        transform.position = shootingPoint.position;
        transform.rotation = shootingPoint.rotation;
        this.speed = speed;
        this.damage = damage;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;

        if (target == null || age >= 10f)
        {
            OriginFactory.Reclaim(this);
            return false;
        }

        Vector3 direction = target.transform.position - transform.position;
        float moveAmount = Time.deltaTime * speed;

        if (direction.magnitude <= moveAmount)
        {
            GameObject impact = Instantiate(impactEffect, target.transform.position, target.transform.rotation);
            Destroy(impact, 2f);

            target.Enemy.ApplyDamage(damage);
            OriginFactory.Reclaim(this);
            return false;
        }

        transform.LookAt(target.Position);
        transform.Translate(transform.forward * moveAmount, Space.World);

        return true;
    }
}
