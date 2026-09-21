using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : EntityBase
{
    [Header("Stats")]
    public float maxStamina = 100f;
    public float invulnerabilityDuration = 1.5f;

    private float currentStamina;
    private float nextDamageTime = 0f;
    private PlayerMovement playerMovement;

    protected override void Start()
    {
        base.Start();
        currentStamina = maxStamina;
        playerMovement = GetComponent<PlayerMovement>();
        if (SystemManager.Instance != null)
        {
            SystemManager.Instance.RegisterPlayer(GetComponent<PlayerInput>());
        }
    }

    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - (amount * Time.deltaTime), 0f, maxStamina);
        Debug.Log($"Stats: Vigor actual: {currentStamina}"); // mandar a UI
    }

    public void RegenerateStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina + (amount * Time.deltaTime), 0f, maxStamina); // mandar a UI
    }

    public bool HasEnoughStamina(float cost)
    {
        return currentStamina >= cost;
    }

    public override void TakeDamage(int damage)
    {
        if (Time.time < nextDamageTime) return;

        base.TakeDamage(damage);
        if (currentHealth > 0)
        {
            nextDamageTime = Time.time + invulnerabilityDuration;
        }
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        Debug.Log($"Stats: ¡Salud recuperada! Salud actual: {currentHealth}"); // mandar a UI
    }

    public bool IsHealthFull()
    {
        return currentHealth == maxHealth;
    }

    protected override void Die()
    {
        if (SystemManager.Instance != null)
        {
            SystemManager.Instance.GameOver();
        }
    }
}