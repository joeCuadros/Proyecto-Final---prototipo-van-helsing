using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Salud")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Resistencia")]
    public float maxStamina = 100f;
    private float currentStamina;

    [Header("Inmunidad")]
    public float invulnerabilityDuration = 1.5f;
    private float nextDamageTime = 0f;

    private PlayerMovement playerMovement;

    void Start()
    {
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        playerMovement = GetComponent<PlayerMovement>();
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

    public void TakeDamage(int damage)
    {
        if (Time.time < nextDamageTime)
        {
            Debug.Log("Stats: Inmune temporalmente, daño ignorado."); // quitar debug
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - damage, 0, maxHealth);
        Debug.Log($"Stats: ¡Daño recibido! Salud restante: {currentHealth}"); // mandar a UI

        if (currentHealth == 0)
        {
            Die();
        }
        else
        {
            nextDamageTime = Time.time + invulnerabilityDuration; //otorgar inmunidad
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

    public void SetAllControls(bool state)
    {
        if (playerMovement != null) playerMovement.SetMovementEnabled(state);

        PlayerCombat combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.SetCombatEnabled(state);

        PlayerInteract interact = GetComponent<PlayerInteract>();
        if (interact != null) interact.SetInteractEnabled(state);
    }

    // Muerte
    private void Die()
    {
        Debug.Log("Stats: ¡Murio!"); // mandar a UI
        SetAllControls(false);
    }
}