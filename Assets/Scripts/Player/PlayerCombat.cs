using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType { None, Dagger, Sword, Crossbow, HolyWater }
public enum AmmoType { None, WoodenStake, SilverBullet }

public class PlayerCombat : MonoBehaviour
{
    [Header("Armas Desbloqueadas (Inventario)")]
    public bool hasDagger = false;
    public bool hasSword = false;
    public bool hasCrossbow = false;

    [Header("Estado del Combate")]
    public WeaponType currentWeapon = WeaponType.None;
    public AmmoType currentAmmo = AmmoType.None;

    [Header("Tiempos y Cooldowns")]
    public float actionCooldown = 1f;
    private float nextActionTime = 0f;

    [Header("Municion (Consumibles)")]
    public int woodenStakes = 0;
    public int silverBullets = 0;
    public int holyWaterFlasks = 0;

    public void EquipWeapon(WeaponType weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        Debug.Log($"Combat: Arma cambiada a {currentWeapon}");
    }

    public void EquipAmmo(AmmoType ammoToEquip)
    {
        currentAmmo = ammoToEquip;
        Debug.Log($"Combat: Municion cambiada a {currentAmmo}");
    }

    void OnAttack()
    {
        if (Time.time < nextActionTime) return;

        switch (currentWeapon)
        {
            case WeaponType.None:
                DodgeRoll();
                break;
            case WeaponType.Dagger:
                if (hasDagger) UseDagger();
                break;
            case WeaponType.Sword:
                if (hasSword) UseSword();
                break;
            case WeaponType.Crossbow:
                if (hasCrossbow) UseCrossbow();
                break;
            case WeaponType.HolyWater:
                UseHolyWater();
                break;
        }
    }

    private void DodgeRoll()
    {
        Debug.Log("Combat: ¡Esquivando/Rodando! (Manos vacías)");
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseDagger()
    {
        Debug.Log("Combat: Atacando con Daga de Plata.");
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseSword()
    {
        Debug.Log("Combat: Tajo letal con Espada de Plata.");
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseCrossbow()
    {
        if (currentAmmo == AmmoType.WoodenStake && woodenStakes > 0)
        {
            woodenStakes--;
            Debug.Log($"Combat: Disparando Estaca (Quedan {woodenStakes}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else if (currentAmmo == AmmoType.SilverBullet && silverBullets > 0)
        {
            silverBullets--;
            Debug.Log($"Combat: Disparando Bala de Plata (Quedan {silverBullets}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else
        {
            Debug.Log("Combat: Sin munición seleccionada o agotada.");
        }
    }

    private void UseHolyWater()
    {
        if (holyWaterFlasks > 0)
        {
            holyWaterFlasks--;
            Debug.Log($"Combat: Lanzando Agua Bendita (Quedan {holyWaterFlasks}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else
        {
            Debug.Log("Combat: No tienes Agua Bendita.");
        }
    }
}