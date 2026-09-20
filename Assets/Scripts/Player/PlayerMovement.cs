using System;
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
    private Vector2 lastInput;
    private bool isSprinting = false;
    private PlayerStats stats;
    // eventos
    public event Action<Vector2> OnDirectionChanged;
    public event Action<bool> OnSprintChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        currentSpeed = walkSpeed;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        if (Mathf.Abs(moveInput.x) > 0 && Mathf.Abs(moveInput.y) > 0)
        {
            if (Mathf.Abs(lastInput.x) > 0)
            {
                moveInput.x = 0f;
                moveInput.y = 1f;
            }
            else
            {
                moveInput.y = 0f;
                moveInput.x = 1f;
            }
        }
        lastInput = moveInput;
        OnDirectionChanged?.Invoke(moveInput);
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
        OnSprintChanged?.Invoke(isSprinting);
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