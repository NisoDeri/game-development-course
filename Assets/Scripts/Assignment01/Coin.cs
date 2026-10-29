using UnityEngine;

public class Coin : MonoBehaviour
{
    // Both are filled in by the CoinSpawner right after Instantiate:
    // a prefab cannot hold a reference to an object that lives in the scene.
    public int value;
    public GameManager gameManager;

    // The player can overlap the coin for a few physics steps; make sure it only counts once.
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
        {
            return;
        }

        // A coin disappears only when the Player touches it (not the enemy, not other coins).
        if (other.CompareTag("Player"))
        {
            collected = true;
            gameManager.CoinCollected(value);
            Destroy(gameObject);
        }
    }
}
