using UnityEngine;

public class EnemyVampire : EnemyBase
{
    protected override void Start()
    {
        maxHealth = 3;
        moveSpeed = 3.5f;
        fearPercentagePer10Sec = 5f;
        prioritizesPlayer = true;
        base.Start();
    }

    public override void TakeDamage(int damage, WeaponType weapon, AmmoType ammo = AmmoType.None)
    {
        if (weapon == WeaponType.Crossbow && ammo == AmmoType.WoodenStake)
        {
            base.TakeDamage(damage, weapon, ammo);
            Debug.Log("Vampiro: Recibe daño por estaca.");
        }
        else
        {
            Debug.Log("Vampiro: Inmune a este ataque.");
        }
    }

    protected override void Attack()
    {
        currentHealth = Mathf.Clamp(currentHealth + 1, 0, maxHealth);
        Debug.Log("Vampiro: ¡Atacó y recuperó 1 corazón!");
    }

    protected override void Die()
    {
        Debug.Log("Vampiro: Destruido.");
        Destroy(gameObject);
    }
}