using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : EntityBase
{
    [Header("Stats")]
    public float maxStamina = 100f;
    public float invulnerabilityDuration = 1.5f;

    private float currentStamina;
    private float nextDamageTime = 0f;

    protected override void Start()
    {
        base.Start();
        currentStamina = maxStamina;
        if (SystemManager.Instance != null) SystemManager.Instance.RegisterPlayer(GetComponent<PlayerInput>());
    }

    public void DrainStamina(float amount)
    {
        currentStamina = Mathf.Clamp(currentStamina - (amount * Time.deltaTime), 0f, maxStamina);
        if (UIManager.Instance != null) UIManager.Instance.ChangeStamina(currentStamina, maxStamina);

    }

    public void RegenerateStamina(float amount)
    {
        if (currentStamina == maxStamina) return;
        currentStamina = Mathf.Clamp(currentStamina + (amount * Time.deltaTime), 0f, maxStamina);
        if (UIManager.Instance != null) UIManager.Instance.ChangeStamina(currentStamina, maxStamina);
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
        if (UIManager.Instance != null) UIManager.Instance.ChangeHealth(currentHealth, maxHealth);
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (UIManager.Instance != null) UIManager.Instance.ChangeHealth(currentHealth, maxHealth);
    }

    public bool IsHealthFull()
    {
        return currentHealth == maxHealth;
    }

    protected override void Die()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.GameOver();
    }
}