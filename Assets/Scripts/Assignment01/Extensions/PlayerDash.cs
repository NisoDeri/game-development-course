using UnityEngine;
using UnityEngine.InputSystem;

// Extension: Space gives a short speed burst. It only works while the player is moving.
// PlayerMovement stays untouched: the dash just raises its speed for a moment.
public class PlayerDash : MonoBehaviour
{
    public float speedMultiplier = 2.5f;
    public float dashDuration = 0.15f;
    public float cooldown = 1.5f;

    private PlayerMovement playerMovement;
    private float normalSpeed;
    private float dashTimeLeft;
    private float cooldownTimeLeft;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (dashTimeLeft > 0f)
        {
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0f)
            {
                playerMovement.speed = normalSpeed;
            }
        }

        if (cooldownTimeLeft > 0f)
        {
            cooldownTimeLeft -= Time.deltaTime;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        bool canDash = dashTimeLeft <= 0f && cooldownTimeLeft <= 0f;

        if (canDash && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // Remember the speed now, so changing it in the Inspector still works between dashes.
            normalSpeed = playerMovement.speed;
            playerMovement.speed = normalSpeed * speedMultiplier;
            dashTimeLeft = dashDuration;
            cooldownTimeLeft = cooldown;
        }
    }

    void OnDisable()
    {
        // Switching the extension off in the middle of a dash must not leave the player fast forever.
        if (dashTimeLeft > 0f && playerMovement != null)
        {
            playerMovement.speed = normalSpeed;
        }
    }
}
