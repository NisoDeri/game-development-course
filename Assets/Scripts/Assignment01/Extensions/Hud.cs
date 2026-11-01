using TMPro;
using UnityEngine;

// Extension: on-screen texts. It only READS the game state through the getters, it never changes the game.
// Remove the HUD object and the game runs exactly as before (the Debug.Log messages stay either way).
public class Hud : MonoBehaviour
{
    public GameManager gameManager;
    public CoinSpawner coinSpawner;
    public KnightEnemy knight;           // optional: leave empty if there is no knight

    public TMP_Text scoreText;
    public TMP_Text batchText;
    public TMP_Text warningText;         // "Knight incoming!"
    public TMP_Text endText;             // "PROMOTED!" / "CAPTURED!"
    public TMP_Text restartText;         // "Press R to restart"

    public float warningTime = 2f;

    private float warningTimeLeft;
    private bool knightWasShown;

    void Update()
    {
        scoreText.text = $"{gameManager.GetScore()} / {gameManager.targetScore}";
        batchText.text = $"Batch {coinSpawner.GetBatchNumber()}";

        // Show the warning once, at the moment the knight appears.
        if (knight != null && knight.HasAppeared() && !knightWasShown)
        {
            knightWasShown = true;
            warningTimeLeft = warningTime;
            warningText.gameObject.SetActive(true);
        }

        if (warningTimeLeft > 0f)
        {
            warningTimeLeft -= Time.deltaTime;

            if (warningTimeLeft <= 0f)
            {
                warningText.gameObject.SetActive(false);
            }
        }

        if (gameManager.IsGameOver())
        {
            warningText.gameObject.SetActive(false);
            endText.gameObject.SetActive(true);
            restartText.gameObject.SetActive(true);

            if (gameManager.IsWon())
            {
                endText.text = "PROMOTED!";
            }
            else
            {
                endText.text = "CAPTURED!";
            }
        }
    }
}
