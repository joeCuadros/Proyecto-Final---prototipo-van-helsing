using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Configuracion")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float staminaCostPerSecond = 20f;
    public float staminaRegenPerSecond = 10f;

    private float currentSpeed;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isSprinting = false;

    private PlayerStats stats;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        currentSpeed = walkSpeed;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    void FixedUpdate()
    {
        float staminaCostThisFrame = staminaCostPerSecond * Time.fixedDeltaTime;
        if (isSprinting && moveInput != Vector2.zero && stats.HasEnoughStamina(staminaCostThisFrame))
        {
            currentSpeed = sprintSpeed;
            stats.DrainStamina(staminaCostPerSecond);
        }
        else
        {
            currentSpeed = walkSpeed;
            stats.RegenerateStamina(staminaRegenPerSecond);
        }

        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }
}