using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth;
    protected int currentHealth;
    public int attackDamage = 1; // Daño base

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage, WeaponType weapon)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} recibió {damage} de daño. Salud restante: {currentHealth}");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Attack(PlayerStats player)
    {
        player.TakeDamage(attackDamage);
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} ha muerto.");
        Destroy(gameObject);
    }
}