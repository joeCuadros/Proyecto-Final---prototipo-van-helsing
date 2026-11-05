using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : EntityBase
{
    [Header("Stats")]
    public float maxStamina = 100f;
    public float invulnerabilityDuration = 1.5f;
    public bool hasShield = false;

    private float currentStamina;
    private float nextDamageTime = 0f;

    [Header("Audio")]
    public AudioClip shieldClip;

    protected override void Start()
    {
        base.Start();
        currentStamina = maxStamina;
        if (SystemManager.Instance != null) SystemManager.Instance.RegisterPlayer(GetComponent<PlayerInput>());
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth, hasShield);
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
        if (hasShield)
        {
            hasShield = false;
        }
        else
        {
            base.TakeDamage(damage);
        }
        if (currentHealth > 0)
        {
            nextDamageTime = Time.time + invulnerabilityDuration;
        }
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth, hasShield);
    }

    public void RegenerateHealth(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth, hasShield);
    }

    public void EquipShield()
    {
        if (hasShield) return; 
        hasShield = true;
        if (AudioManager.Instance != null)
              AudioManager.Instance.PlaySFX(shieldClip);
        if (UIManager.Instance != null) UIManager.Instance.UpdateHealthUI(currentHealth, maxHealth, hasShield);
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