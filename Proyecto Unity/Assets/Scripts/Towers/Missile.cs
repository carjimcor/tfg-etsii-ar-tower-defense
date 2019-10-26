using UnityEngine;

public class Missile : WarEntity
{
    [SerializeField, Range(0.01f, 1f)]
    float minimumDetonationDistance = 0.1f;

    float age;

    TargetPoint target;

    float speed, blastRadius, damage;

    [SerializeField]
    Transform tail = default;

    public void Initialize(
        TargetPoint target, Transform _transform,
        float speed, float blastRadius, float damage
    )
    {
        this.target = target;
        transform.position = _transform.position;
        transform.rotation = _transform.rotation;
        this.speed = speed;
        this.blastRadius = blastRadius;
        this.damage = damage;
    }

    public override bool GameUpdate()
    {
        age += Time.deltaTime;

        if (target == null || age >= 10f)
        {
            Game.SpawnExplosion().Initialize(transform.position, .5f, 0);

            OriginFactory.Reclaim(this);
            return false;
        }

        if (Vector3.Distance(transform.position, target.Position) <= minimumDetonationDistance)
        {
            Game.SpawnExplosion().Initialize(transform.position, blastRadius, damage);

            OriginFactory.Reclaim(this);
            return false;
        }

        transform.LookAt(target.Position);
        transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);

        Game.SpawnExplosion().Initialize(tail.position,0.1f);

        return true;
    }

}