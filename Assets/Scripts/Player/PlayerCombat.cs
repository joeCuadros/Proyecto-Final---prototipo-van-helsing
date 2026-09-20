using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponType { Dagger, WoodenStake, SilverBullet, HolyWater }

public class PlayerCombat : MonoBehaviour
{
    [Header("Selección Actual")]
    public WeaponType currentWeapon = WeaponType.Dagger;

    [Header("Tiempos y Cooldowns")]
    public float actionCooldown = 1f;
    private float nextActionTime = 0f;

    [Header("Equipamiento")]
    public bool hasDagger = true;
    public int woodenStakes = 0;
    public int silverBullets = 0;
    public int holyWaterFlasks = 0;

    // CAMBIOS DIRECTOS (TECLAS 1, 2, 3, 4)
    void OnEquipDagger() => EquipWeapon(WeaponType.Dagger);
    void OnEquipStake() => EquipWeapon(WeaponType.WoodenStake);
    void OnEquipBullet() => EquipWeapon(WeaponType.SilverBullet);
    void OnEquipHolyWater() => EquipWeapon(WeaponType.HolyWater);

    public void EquipWeapon(WeaponType weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        Debug.Log($"Combat: Has equipado -> {currentWeapon}");
    }

    // LOGICA DE ATAQUE
    void OnAttack()
    {
        if (Time.time < nextActionTime) return;

        switch (currentWeapon)
        {
            case WeaponType.Dagger:
                if (hasDagger) UseDagger();
                break;
            case WeaponType.WoodenStake:
                UseWoodenStake();
                break;
            case WeaponType.SilverBullet:
                UseSilverBullet();
                break;
            case WeaponType.HolyWater:
                UseHolyWater();
                break;
        }
    }

    private void UseDagger()
    {
        Debug.Log("Combat: Atacando con Daga de Plata (Infinito).");
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseWoodenStake()
    {
        if (woodenStakes > 0)
        {
            woodenStakes--;
            Debug.Log($"Combat: Disparando Estaca (Finito - Quedan {woodenStakes}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else
        {
            Debug.Log("Combat: No tienes Estacas de madera.");
        }
    }

    private void UseSilverBullet()
    {
        if (silverBullets > 0)
        {
            silverBullets--;
            Debug.Log($"Combat: Disparando Bala de Plata (Finito - Quedan {silverBullets}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else
        {
            Debug.Log("Combat: No tienes Balas de plata.");
        }
    }

    private void UseHolyWater()
    {
        if (holyWaterFlasks > 0)
        {
            holyWaterFlasks--;
            Debug.Log($"Combat: Lanzando Agua Bendita (Finito - Quedan {holyWaterFlasks}).");
            nextActionTime = Time.time + actionCooldown;
        }
        else
        {
            Debug.Log("Combat: No tienes Agua Bendita.");
        }
    }
}