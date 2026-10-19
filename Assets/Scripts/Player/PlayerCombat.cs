using System;
using UnityEngine;

public enum WeaponType { Dagger, WoodenStake, SilverBullet, HolyWater }

public class PlayerCombat : MonoBehaviour
{

    [Header("Selección Actual")]
    public WeaponType currentWeapon = WeaponType.Dagger;

    [Header("Audio")]
    public AudioClip audioDagger;
    public AudioClip audioStakes;
    public AudioClip audioCrossbow;
    public AudioClip audioWater;


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

    [Header("Bala de plata")]
    public Projectile bulletPrefab;
    public int bulletDamage = 1;

    [Header("Estaca de madera")]
    public Projectile stakePrefab;
    public int stakeDamage = 1;

    [Header("Agua bendita")]
    public Projectile holyWaterPrefab;
    public int holyWaterDamage = 1;

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
                enemy.TakeDamage(daggerDamage, WeaponType.Dagger);
            }
        }
        nextActionTime = Time.time + actionCooldown;
        if(AudioManager.Instance != null && audioDagger != null)
        {
            AudioManager.Instance.PlaySFX(audioDagger);
        }
    }

    private void UseWoodenStake()
    {
        woodenStakes--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();

        if (stakePrefab != null)
        {
            Projectile stake = Instantiate(stakePrefab, transform.position, Quaternion.identity);
            stake.Launch(attackDirection, stakeDamage);
        }

        if (AudioManager.Instance != null && audioCrossbow != null)
        {
            AudioManager.Instance.PlaySFX(audioCrossbow);
        }
    }

    private void UseSilverBullet()
    {
        silverBullets--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();

        if (bulletPrefab != null)
        {
            Projectile bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            bullet.Launch(attackDirection, bulletDamage);
        }

        if (AudioManager.Instance != null && audioStakes != null)
        {
            AudioManager.Instance.PlaySFX(audioStakes);
        }
    }

    private void UseHolyWater()
    {
        holyWaterFlasks--;
        nextActionTime = Time.time + actionCooldown;
        UpdateWeaponsUI();

        if (holyWaterPrefab != null)
        {
            Projectile flask = Instantiate(holyWaterPrefab, transform.position, Quaternion.identity);
            flask.Launch(attackDirection, holyWaterDamage);
        }

        if (AudioManager.Instance != null && audioWater != null)
        {
            AudioManager.Instance.PlaySFX(audioWater);
        }
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