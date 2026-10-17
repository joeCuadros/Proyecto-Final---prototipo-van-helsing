using System;
using UnityEngine;

public enum WeaponType { Dagger, WoodenStake, SilverBullet, HolyWater }

public class PlayerCombat : MonoBehaviour
{

    [Header("Selección Actual")]
    public WeaponType currentWeapon = WeaponType.Dagger;

    [Header("Audio")]
    public AudioClip music;

    [Header("Tiempos y Cooldowns")]
    public float actionCooldown = 1f;
    private float nextActionTime = 0f;

    [Header("Equipamiento")]
    public bool hasDagger = true;
    public int woodenStakes = 0;
    public int silverBullets = 0;
    public int holyWaterFlasks = 0;

    [Header("Daga")]
    public float daggerRange = 1.5f;
    public float daggerAngle = 90f;
    public int daggerDamage = 1;
    public LayerMask enemyLayer;

    private PlayerMovement movement;
    private Vector2 attackDirection = Vector2.down;
    public event Action<WeaponType> OnAttackFired;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }
    void Start()
    {
        UpdateWeaponsUI();
    }
    void OnEnable()
    {
        if (movement != null)
            movement.OnDirectionChanged += UpdateAttackDirection;
    }

    void OnDisable()
    {
        if (movement != null)
            movement.OnDirectionChanged -= UpdateAttackDirection;
    }
    void UpdateAttackDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
            attackDirection = direction.normalized;
    }

    // CAMBIOS DIRECTOS (TECLAS 1, 2, 3, 4)
    void OnEquipDagger() => EquipWeapon(WeaponType.Dagger);
    void OnEquipStake() => EquipWeapon(WeaponType.WoodenStake);
    void OnEquipBullet() => EquipWeapon(WeaponType.SilverBullet);
    void OnEquipHolyWater() => EquipWeapon(WeaponType.HolyWater);

    public void EquipWeapon(WeaponType weaponToEquip)
    {
        currentWeapon = weaponToEquip;
        Debug.Log($"Combat: Has equipado -> {currentWeapon}"); // Mandar a UI
        UpdateWeaponsUI();
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
        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            transform.position,
            daggerRange,
            enemyLayer
        );

        foreach (Collider2D enemyCollider in enemies)
        {
            EntityBase enemy = enemyCollider.GetComponentInParent<EntityBase>();

            if (enemy == null)
                continue;

            Vector2 directionToEnemy =
                (enemy.transform.position - transform.position).normalized;

            float angle = Vector2.Angle(
                attackDirection,
                directionToEnemy
            );

            if (angle <= daggerAngle / 2f)
            {
                enemy.TakeDamage(daggerDamage);
            }
        }
        nextActionTime = Time.time + actionCooldown;
        if(AudioManager.Instance != null && music != null)
        {
            AudioManager.Instance.PlaySFX(music);
        }
    }

    private void UseWoodenStake()
    {
        woodenStakes--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();
    }

    private void UseSilverBullet()
    {
        silverBullets--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();
    }

    private void UseHolyWater()
    {
        holyWaterFlasks--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();
    }
    private void UpdateWeaponsUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.RefreshWeaponsUI(currentWeapon, hasDagger, woodenStakes, silverBullets, holyWaterFlasks);
        }
    }
    // DEBUG
    private void OnDrawGizmosSelected()
    {
        if (currentWeapon != WeaponType.Dagger)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            daggerRange
        );

        Vector2 leftDirection = Quaternion.Euler(
            0f,
            0f,
            daggerAngle / 2f
        ) * attackDirection;

        Vector2 rightDirection = Quaternion.Euler(
            0f,
            0f,
            -daggerAngle / 2f
        ) * attackDirection;

        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)leftDirection * daggerRange
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)rightDirection * daggerRange
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)attackDirection * daggerRange
        );
    }
}