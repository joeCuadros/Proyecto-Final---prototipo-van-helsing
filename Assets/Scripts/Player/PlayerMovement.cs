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
    private bool isSprinting = false;
    private PlayerStats stats;
    private PlayerCarryCivilian carry;
    // eventos
    public event Action<Vector2> OnDirectionChanged;
    public event Action<bool> OnSprintChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        currentSpeed = walkSpeed;
        carry = GetComponent<PlayerCarryCivilian>();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
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
        // 1. Corriendo
        if (isSprinting && moveInput != Vector2.zero && stats.HasEnoughStamina(staminaCostThisFrame))
        {

            float multiplier =
                carry != null && carry.IsCarrying
                    ? carry.SprintMultiplier
                    : 1f;

            currentSpeed = sprintSpeed * multiplier;

            stats.DrainStamina(staminaCostPerSecond);
        }
        // 2. Caminando
        else if (moveInput != Vector2.zero)
        {
            currentSpeed = walkSpeed;
        }
        // 3. Quieto
        else
        {
            stats.RegenerateStamina(staminaRegenPerSecond);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }
}