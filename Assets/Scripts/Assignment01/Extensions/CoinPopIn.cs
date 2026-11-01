using UnityEngine;

// Extension: a new coin grows from (almost) nothing to full size. Untick or remove this component and coins simply appear.
public class CoinPopIn : MonoBehaviour
{
    public float duration = 0.2f;

    private Vector3 fullScale;
    private float timer;

    void Awake()
    {
        // Awake also runs when the component is unticked, so here we only remember the size.
        fullScale = transform.localScale;
    }

    void OnEnable()
    {
        // OnEnable runs inside Instantiate (the coin is tiny before its first frame is drawn),
        // but NOT for an unticked component, which keeps the coin at full size.
        timer = 0f;
        transform.localScale = fullScale * 0.01f;
    }

    void OnDisable()
    {
        // Unticking the component while a coin is growing gives back the normal full-size coin.
        transform.localScale = fullScale;
    }

    void Update()
    {
        if (timer >= duration)
        {
            return;
        }

        timer += Time.deltaTime;

        // Never exactly 0: a zero-size collider is pointless, and the coin can already be collected while it grows.
        float progress = Mathf.Min(timer / duration, 1f);
        if (progress < 0.01f)
        {
            progress = 0.01f;
        }

        transform.localScale = fullScale * progress;
    }
}
