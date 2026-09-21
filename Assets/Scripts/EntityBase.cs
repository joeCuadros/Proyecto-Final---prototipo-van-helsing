using UnityEngine;


public abstract class EntityBase : MonoBehaviour
{
    [Header("Stats Base")]
    public int maxHealth;
    protected int currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        Debug.Log($"{gameObject.name} recibió {damage}. Vida: {currentHealth}");
        if (currentHealth == 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}