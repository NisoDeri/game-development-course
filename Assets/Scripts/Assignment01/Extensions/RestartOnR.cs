using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Extension: after a win or a loss, R loads the scene again from scratch (score, coins and enemies reset).
public class RestartOnR : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        // Restarting in the middle of a game is not allowed, only after it ended.
        if (!gameManager.IsGameOver())
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
