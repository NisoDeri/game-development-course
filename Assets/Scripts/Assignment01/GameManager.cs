using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int targetScore = 1000;
    public CoinSpawner coinSpawner;
    public GameObject player;
    public GameObject enemy;

    private int score;

    // Once the game is won or lost, nothing else may be counted and no second message is printed.
    private bool isGameOver;

    // Called by a Coin when the player touches it.
    public void CoinCollected(int value)
    {
        if (isGameOver)
        {
            return;
        }

        score += value;
        Debug.Log($"+{value} → {score}/{targetScore}");

        if (score >= targetScore)
        {
            // The win can happen in the middle of a batch, so no new batch is requested.
            isGameOver = true;
            Debug.Log("you won");
            Destroy(enemy);
            return;
        }

        coinSpawner.OnCoinCollected();
    }

    // Called by the enemy when it touches the player.
    public void PlayerCaught()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        Debug.Log("you lose");
        Destroy(player);
    }
}
