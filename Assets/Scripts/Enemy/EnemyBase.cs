using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Estadísticas Base")]
    public int maxHealth;
    protected int currentHealth;
    public float moveSpeed;

    [Header("Miedo")]
    public float fearPercentagePer10Sec;

    [Header("Detección y Prioridad")]
    public float visionRadius = 5f;
    protected bool prioritizesPlayer = true;
    protected Transform currentTarget;

    protected enum EnemyState { Wandering, Alert, Seeking }
    protected EnemyState currentState = EnemyState.Wandering;

    private Vector2 randomWanderPoint;
    private float wanderTimer;

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        SetNewWanderPoint();
    }

    protected virtual void Update()
    {
        CheckForTargets();

        switch (currentState)
        {
            case EnemyState.Wandering: Patrol(); break;
            case EnemyState.Alert: Investigate(); break;
            case EnemyState.Seeking: ChaseTarget(); break;
        }
    }

    public virtual void TakeDamage(int damage, WeaponType weapon, AmmoType ammo = AmmoType.None)
    {
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    protected virtual void CheckForTargets()
    {
        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance > visionRadius * 1.5f)
            {
                currentTarget = null;
                currentState = EnemyState.Wandering;
                SetNewWanderPoint();
            }
            return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius);

        Transform foundPlayer = null;
        Transform foundCivilian = null;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player")) foundPlayer = hit.transform;
            else if (hit.CompareTag("Civilian")) foundCivilian = hit.transform;
        }

        if (foundPlayer != null || foundCivilian != null)
        {
            if (prioritizesPlayer)
            {
                currentTarget = foundPlayer != null ? foundPlayer : foundCivilian;
            }
            else
            {
                currentTarget = foundCivilian != null ? foundCivilian : foundPlayer;
            }

            currentState = EnemyState.Seeking;
        }
    }

    protected abstract void Attack();
    protected abstract void Die();

    protected virtual void Patrol()
    {
        transform.position = Vector2.MoveTowards(transform.position, randomWanderPoint, (moveSpeed * 0.5f) * Time.deltaTime);

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f || Vector2.Distance(transform.position, randomWanderPoint) < 0.2f)
        {
            SetNewWanderPoint();
        }
    }

    private void SetNewWanderPoint()
    {
        randomWanderPoint = (Vector2)transform.position + Random.insideUnitCircle * 3f;
        wanderTimer = 3f;
    }

    protected virtual void Investigate() { /* Ir al ruido */ }

    protected virtual void ChaseTarget()
    {
        if (currentTarget != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRadius);
    }
}