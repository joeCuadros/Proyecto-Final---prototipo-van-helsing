using System;
using UnityEngine;

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

    public event Action<WeaponType> OnAttackFired;

    // CAMBIOS DIRECTOS (TECLAS 1, 2, 3, 4)
    void OnEquipDagger() => EquipWeapon(WeaponType.Dagger);
    void OnEquipStake() => EquipWeapon(WeaponType.WoodenStake);
    void OnEquipBullet() => EquipWeapon(WeaponType.SilverBullet);
    void OnEquipHolyWater() => EquipWeapon(WeaponType.HolyWater);

    public void EquipWeapon(WeaponType weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        Debug.Log($"Combat: Has equipado -> {currentWeapon}"); // Mandar a UI
    }

    // LOGICA DE ATAQUE
    void OnAttack()
    {
        if (Time.time < nextActionTime) return;
        switch (currentWeapon)
        {
            case WeaponType.Dagger:
                if (!hasDagger) return;
                UseDagger();
                break;
            case WeaponType.WoodenStake:
                if (woodenStakes <= 0) return; 
                UseWoodenStake();
                break;
            case WeaponType.SilverBullet:
                if (silverBullets <= 0) return;
                UseSilverBullet();
                break;
            case WeaponType.HolyWater:
                if (holyWaterFlasks <= 0) return;
                UseHolyWater();
                break;
        }
        OnAttackFired?.Invoke(currentWeapon);
    }

    private void UseDagger()
    {
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseWoodenStake()
    {
        woodenStakes--;
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseSilverBullet()
    {
        silverBullets--;
        nextActionTime = Time.time + actionCooldown;
    }

    private void UseHolyWater()
    {
        holyWaterFlasks--;
        nextActionTime = Time.time + actionCooldown;
    }
}