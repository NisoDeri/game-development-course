using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public int coinsPerBatch = 5;

    // The random position of every coin is chosen between these two corners.
    public Vector2 spawnMin = new Vector2(-7.6f, -3.8f);
    public Vector2 spawnMax = new Vector2(7.6f, 3.8f);

    // Coin type i has the value coinValues[i] and the color coinColors[i].
    public int[] coinValues = { 50, 20, 10 };
    public Color[] coinColors =
    {
        new Color(0.949f, 0.549f, 0.157f), // orange
        new Color(0.235f, 0.702f, 0.443f), // green
        new Color(0.227f, 0.482f, 0.835f)  // blue
    };

    public GameManager gameManager;

    private int coinsRemaining;

    void Start()
    {
        // Warn once, here, instead of every time a coin is created.
        if (coinValues.Length != coinColors.Length)
        {
            Debug.LogWarning("CoinSpawner: coinValues and coinColors should have the same length.");
        }

        SpawnBatch();
    }

    // Called by the GameManager each time the player collects a coin.
    public void OnCoinCollected()
    {
        coinsRemaining--;

        // The whole batch was collected -> hand out the next one.
        if (coinsRemaining == 0)
        {
            SpawnBatch();
        }
    }

    void SpawnBatch()
    {
        coinsRemaining = coinsPerBatch;

        for (int i = 0; i < coinsPerBatch; i++)
        {
            SpawnCoin();
        }
    }

    void SpawnCoin()
    {
        // Random.Range with ints: the second number is exclusive, so this gives 0 .. Length-1.
        int type = Random.Range(0, coinValues.Length);

        float x = Random.Range(spawnMin.x, spawnMax.x);
        float y = Random.Range(spawnMin.y, spawnMax.y);
        Vector3 position = new Vector3(x, y, 0f);

        GameObject newCoin = Instantiate(coinPrefab, position, Quaternion.identity);

        Coin coin = newCoin.GetComponent<Coin>();
        coin.value = coinValues[type];
        coin.gameManager = gameManager;

        // If the arrays have different lengths we keep the prefab color instead of crashing.
        if (type < coinColors.Length)
        {
            newCoin.GetComponent<SpriteRenderer>().color = coinColors[type];
        }
    }
}
