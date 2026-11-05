using System;
using UnityEngine;


public abstract class EntityBase : MonoBehaviour
{
    [Header("Stats Base")]
    public int maxHealth = 3;
    protected int currentHealth;
    public int CurrentHealth => currentHealth;
    public event Action<int, int> HealthChanged;
    public event Action<int> Damaged;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        NotifyHealth();
    }

    protected void NotifyHealth()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public virtual void TakeDamage(int damage)
    {
        if (damage <= 0 || currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        NotifyHealth();
        Damaged?.Invoke(damage);

        Debug.Log(
            $"{gameObject.name} recibió {damage} de daño. " +
            $"Vida restante: {currentHealth}"
        );

        if (currentHealth == 0) Die();
    }
    public virtual void TakeDamage(int damage, WeaponType weapon)
    {
        TakeDamage(damage);
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}