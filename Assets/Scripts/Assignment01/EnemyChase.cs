using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    public float speed = 4.5f;
    public GameObject player;
    public GameManager gameManager;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // The player is destroyed when the enemy catches it, so there is nobody left to chase.
        if (player == null)
        {
            return;
        }

        Vector2 playerPosition = player.transform.position;
        Vector2 direction = (playerPosition - rb.position).normalized;

        // speed * fixedDeltaTime is applied exactly once; multiplying twice would slow the enemy down.
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only the player counts. Coins and walls are ignored.
        if (other.CompareTag("Player"))
        {
            gameManager.PlayerCaught();
        }
    }
}
