using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public float maxStamina = 100f;
    public float invulnerabilityDuration = 1.5f;

    private int currentHealth;
    private float currentStamina;
    private float nextDamageTime = 0f;
    private PlayerMovement playerMovement;

    void Start()
    {
        currentHealth = maxHealth;
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

    // Muerte
    private void Die()
    {
        Debug.Log("Stats: ¡Murio!"); // mandar a UI
        // Notificar a SystemMananger
    }
}