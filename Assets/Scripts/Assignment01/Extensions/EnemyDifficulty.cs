using UnityEngine;

// Extension: the red enemy gets a bit faster with every new batch of coins.
public class EnemyDifficulty : MonoBehaviour
{
    public float speedIncreasePerBatch = 0.05f;   // 0.05 = 5% faster for each new batch
    public float maxSpeedPercentOfPlayer = 0.9f;  // 0.9 = never faster than 90% of the player's speed
    public CoinSpawner coinSpawner;
    public PlayerMovement playerMovement;

    private EnemyChase enemyChase;
    private float maxSpeed;

    // Batch 1 is the start of the game, not a "new" batch.
    private int lastBatch = 1;

    void Awake()
    {
        enemyChase = GetComponent<EnemyChase>();
    }

    void Start()
    {
        // Read once: PlayerDash changes the player's speed for a moment, which must not move the limit.
        maxSpeed = playerMovement.speed * maxSpeedPercentOfPlayer;
    }

    void Update()
    {
        int batch = coinSpawner.GetBatchNumber();

        // A loop, so no batch is missed even if two were handed out between two frames.
        while (lastBatch < batch)
        {
            lastBatch++;
            float newSpeed = enemyChase.speed * (1f + speedIncreasePerBatch);

            if (newSpeed > maxSpeed)
            {
                newSpeed = maxSpeed;
            }

            // Never slow the enemy down: a speed set above the cap in the Inspector stays as it is.
            if (newSpeed > enemyChase.speed)
            {
                enemyChase.speed = newSpeed;
            }
        }
    }
}
