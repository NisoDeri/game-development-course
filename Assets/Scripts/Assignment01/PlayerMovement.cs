using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 6f;

    private Rigidbody2D rb;
    private Vector2 moveDirection;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Input is read every frame, but the movement itself happens in FixedUpdate.
        moveDirection = Vector2.zero;

        // No keyboard connected -> stand still instead of throwing a null reference.
        if (Keyboard.current == null)
        {
            return;
        }

        float horizontal = Keyboard.current.dKey.value - Keyboard.current.aKey.value;
        float vertical = Keyboard.current.wKey.value - Keyboard.current.sKey.value;

        // Normalized, otherwise moving diagonally (W + D) would be faster than moving straight.
        moveDirection = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        // MovePosition belongs in FixedUpdate so it stays in sync with the physics system (triggers, walls).
        rb.MovePosition(rb.position + moveDirection * speed * Time.fixedDeltaTime);
    }
}
