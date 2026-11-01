using UnityEngine;

// Extension: a chess knight that joins the chase when a later batch starts.
// It hops in an L-shape to the landing cell closest to the player and warns with a marker first.
// The builder saves it hidden (renderers and collider off), so without this script the game is the original one.
public class KnightEnemy : MonoBehaviour
{
    public GameManager gameManager;
    public CoinSpawner coinSpawner;
    public GameObject player;
    public GameObject marker;          // semi-transparent square shown on the landing cell
    public SpriteRenderer[] visuals;   // everything that has to be shown when the knight appears

    public int appearAtBatch = 3;
    public float moveInterval = 1.2f;  // seconds between two hops
    public float markerTime = 0.5f;    // how long the landing cell is marked before the hop
    public float hopDuration = 0.15f;  // the hop itself is quick

    // Centers of the corner cells. Cells are 1 x 1 units, so the knight always lands on a cell center.
    public Vector2 gridMin = new Vector2(-7.5f, -3.5f);
    public Vector2 gridMax = new Vector2(7.5f, 3.5f);

    private Rigidbody2D rb;
    private Collider2D knightCollider;

    private bool hasAppeared;
    private bool stopped;

    // 0 = choose a landing cell, 1 = marker is shown, 2 = hopped and waiting for the next turn.
    private int step;
    private float cycleTimer;
    private Vector2 targetCell;

    private bool isHopping;
    private float hopTimer;
    private Vector2 hopStart;

    // The 8 L-moves of a knight: 2 cells in one direction and 1 cell in the other.
    private Vector2[] moves =
    {
        new Vector2(1f, 2f), new Vector2(2f, 1f), new Vector2(2f, -1f), new Vector2(1f, -2f),
        new Vector2(-1f, -2f), new Vector2(-2f, -1f), new Vector2(-2f, 1f), new Vector2(-1f, 2f)
    };

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        knightCollider = GetComponent<Collider2D>();
    }

    // Used by the HUD to show the "Knight incoming!" warning.
    public bool HasAppeared()
    {
        return hasAppeared;
    }

    void Update()
    {
        if (gameManager.IsGameOver())
        {
            if (!stopped)
            {
                StopKnight();
            }
            return;
        }

        if (!hasAppeared)
        {
            if (coinSpawner.GetBatchNumber() >= appearAtBatch)
            {
                Appear();
            }
            return;
        }

        if (step == 0)
        {
            targetCell = ChooseTargetCell();
            marker.transform.position = new Vector3(targetCell.x, targetCell.y, 0f);
            marker.SetActive(true);
            cycleTimer = 0f;
            step = 1;
        }

        cycleTimer += Time.deltaTime;

        if (step == 1 && cycleTimer >= markerTime)
        {
            // Time is up: hide the marker and start the hop (it is carried out in FixedUpdate).
            marker.SetActive(false);
            hopStart = rb.position;
            hopTimer = 0f;
            isHopping = true;
            step = 2;
        }
        else if (step == 2 && cycleTimer >= moveInterval)
        {
            step = 0;
        }
    }

    void FixedUpdate()
    {
        if (!isHopping)
        {
            return;
        }

        // Same rule as the player and the enemy: move the Rigidbody2D inside FixedUpdate.
        hopTimer += Time.fixedDeltaTime;
        float progress = Mathf.Min(hopTimer / hopDuration, 1f);
        rb.MovePosition(Vector2.Lerp(hopStart, targetCell, progress));

        if (progress >= 1f)
        {
            isHopping = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.PlayerCaught();
        }
    }

    void Appear()
    {
        hasAppeared = true;

        Vector2 corner = FarthestCorner();
        rb.position = corner;
        transform.position = new Vector3(corner.x, corner.y, 0f);

        SetVisible(true);
        step = 0;
    }

    void StopKnight()
    {
        stopped = true;
        isHopping = false;
        marker.SetActive(false);

        // Like the red enemy, the knight disappears when the player wins.
        if (gameManager.IsWon())
        {
            SetVisible(false);
        }
    }

    void SetVisible(bool visible)
    {
        for (int i = 0; i < visuals.Length; i++)
        {
            visuals[i].enabled = visible;
        }

        knightCollider.enabled = visible;
    }

    Vector2 FarthestCorner()
    {
        Vector2[] corners =
        {
            new Vector2(gridMin.x, gridMin.y), new Vector2(gridMin.x, gridMax.y),
            new Vector2(gridMax.x, gridMin.y), new Vector2(gridMax.x, gridMax.y)
        };

        Vector2 playerPosition = player.transform.position;
        Vector2 best = corners[0];
        float bestDistance = -1f;

        for (int i = 0; i < corners.Length; i++)
        {
            float distance = Vector2.Distance(corners[i], playerPosition);
            if (distance > bestDistance)
            {
                bestDistance = distance;
                best = corners[i];
            }
        }

        return best;
    }

    // Of the L-moves that stay on the board, take the one that lands closest to the player.
    Vector2 ChooseTargetCell()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 best = rb.position;
        float bestDistance = 1000f; // larger than any distance on the board, so the first legal cell always wins

        for (int i = 0; i < moves.Length; i++)
        {
            Vector2 candidate = rb.position + moves[i];

            // A small margin so floating point rounding never rejects a legal cell.
            bool insideX = candidate.x >= gridMin.x - 0.01f && candidate.x <= gridMax.x + 0.01f;
            bool insideY = candidate.y >= gridMin.y - 0.01f && candidate.y <= gridMax.y + 0.01f;
            if (!insideX || !insideY)
            {
                continue;
            }

            float distance = Vector2.Distance(candidate, playerPosition);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }
}
