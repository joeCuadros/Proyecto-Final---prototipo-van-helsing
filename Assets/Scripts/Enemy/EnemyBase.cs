using UnityEngine;

public abstract class EnemyBase : EntityBase
{
    public enum EnemyState
    {
        Idle,       // Quedarse quieto o inactivo
        Wander,     // Patrullar 
        Seek,       // Perseguir a un objetivo
        Flee,       // Huir de un objetivo
        PreAttack,  // Antes de atacar avisar
        Attack,     // Atacar al objetivo
        Cooldown,   // Tiempo despues de atacar
        Stun,       // Stun dejarlo quieto por Stun
        Die         // Muer
    }

    [Header("Configuracion IA")]
    public EnemyState currentState = EnemyState.Idle;
    public float moveSpeed = 3f;
    public int attackDamage = 1;
    protected Transform target;

    public EnemyState combatState;

    protected virtual void Update()
    {
        if (currentState == EnemyState.Die) return;

        switch (currentState)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Wander: UpdateWander(); break;
            case EnemyState.Seek: UpdateSeek(); break;
            case EnemyState.Flee: UpdateFlee(); break;
            case EnemyState.PreAttack: UpdatePreAttack(); break;
            case EnemyState.Attack: UpdateAttack(); break;
            case EnemyState.Cooldown: UpdateCooldown(); break;
            case EnemyState.Stun: UpdateStun(); break;
        }
    }

    // Metodos a sobre escribir
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateWander() { }
    protected virtual void UpdateSeek() { }
    protected virtual void UpdateFlee() { }
    protected virtual void UpdatePreAttack() { }
    protected virtual void UpdateAttack() { }
    protected virtual void UpdateCooldown() { }
    protected virtual void UpdateStun() { }

    public virtual void AttackTarget(EntityBase target)
    {
        target.TakeDamage(attackDamage);
    }
    protected override void Die()
    {
        currentState = EnemyState.Die;
        Debug.Log($"{gameObject.name} murió.");
        Destroy(gameObject, 0.5f);
    }
}