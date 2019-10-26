using UnityEngine;
using UnityEngine.UI;

public class Enemy : GameBehaviour
{

    EnemyFactory originFactory;

    GameTile tileFrom, tileTo;
    Vector3 positionFrom, positionTo;
    float progress;

    //[SerializeField]
    //Transform model = default;

    [SerializeField]
    float startingHealth = 100f;
    float currentHealth;

    [SerializeField]
    int credits = 5;

    [SerializeField]
    float speed = 1f;
    float baseSpeed = 1f;

    public Image healthBar;

    private void Awake()
    {
        currentHealth = startingHealth;
        baseSpeed = speed;
    }

    public EnemyFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory!");
            originFactory = value;
        }
    }

    public void SpawnOn(GameTile tile)
    {
        Debug.Assert(tile.nextOnPath != null, "Nowhere to go!", this);
        tileFrom = tile;
        tileTo = tile.nextOnPath;
        positionFrom = tileFrom.transform.localPosition;
        positionTo = tileTo.transform.localPosition;
        transform.position = tile.transform.position;
        transform.localRotation = tileFrom.PathDirection.GetRotation();
        progress = 0f;
    }

    public void UpdateHealth(float multiplier)
    {
        startingHealth *= multiplier;
        currentHealth = startingHealth;
    }

    public void SlowDown(float percentage)
    {
        // Only 1 slow tower will affect each enemy, the one with the highest slow percentage
        float speed_after = baseSpeed - baseSpeed * percentage;

        if (speed > speed_after)
        {
            speed = speed_after;
        }
    }

    public override bool GameUpdate()
    {
        if (currentHealth <= 0f)
        {
            Game.EnemyDestroyed(credits);
            OriginFactory.Reclaim(this);
            return false;
        }
        progress += Time.deltaTime * speed;
        while (progress >= 1f)
        {
            tileFrom = tileTo;
            tileTo = tileTo.nextOnPath;
            if (tileTo == null)
            {
                Game.ReceiveDamage();
                OriginFactory.Reclaim(this);
                return false;
            }
            positionFrom = positionTo;
            positionTo = tileTo.transform.localPosition;
            transform.localRotation = tileFrom.PathDirection.GetRotation();
            progress -= 1f;
        }
        transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
        speed = baseSpeed;
        return true;
    }

    public void ApplyDamage(float damage)
    {
        Debug.Assert(damage >= 0f, "Negative damage applied.");
        currentHealth -= damage;

        healthBar.fillAmount = currentHealth / startingHealth;

    }
}