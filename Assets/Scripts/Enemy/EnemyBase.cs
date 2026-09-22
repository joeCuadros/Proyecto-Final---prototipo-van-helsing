using UnityEngine;

public abstract class EnemyBase : EntityBase
{
    public enum EnemyState
    {
        Die,        // Muere y deja de ejecutar comportamientos
        Idle,       // Permanece quieto
        Seek,       // Acercarse al objetivo
        Flee,       // Alejarse del objetivo
        Wander,     // Deambular libremente por el area
        Patrol,     // Patrullar puntos especificos en el mapa
        Arrive,     // Se acerca al objetivo reduciendo progresivamente la velocidad
        Attack,     // Ejecutar el ataque contra el objetivo
        Cooldown,   // Espera antes de poder realizar otro ataque
        Stun,       // Que incapacitado temporalmente
    }
    public enum TargetPriority
    {
        None,
        Player,
        NPC
    }

    [Header("Configuracion IA")]
    public EnemyState currentState = EnemyState.Idle;
    
    [Header("Combate")]
    public int attackDamage = 1;
    public float attackRange = 1.5f;
    public float preAttackDuration = 1f;
    public float attackCooldown = 1f;

    [Header("Wander")]
    public float wanderRadius = 2f;
    public float wanderDistance = 2f;
    public float wanderChange = 10f;
    protected Vector2 wanderDirection; // no publico

    [Header("Patrol")]
    public Vector2[] patrolPoints;
    public float patrolPointDistance = 0.2f;
    protected int currentPatrolPoint = 0; // no publico

    [Header("Movimiento")]
    public float maxSpeed = 3f;
    public float acceleration = 4f;
    public float deceleration = 6f;
    protected Vector2 velocity; // no publico

    [Header("Deteccion")]
    public float detectionRadius = 5f;
    public LayerMask npcLayer;
    public LayerMask playerLayer;
    public TargetPriority targetPriority;
    public bool changeTarget = true;
    protected EntityBase target; // no publico

    // GENERALES
    protected Rigidbody2D rb;
    protected float timer = 0f;
        
    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }
    protected virtual void FixedUpdate()
    {
        if (currentState == EnemyState.Die) return;

        switch (currentState)
        {
            case EnemyState.Idle: UpdateIdle(); break;
            case EnemyState.Seek: UpdateSeek(); break;
            case EnemyState.Flee: UpdateFlee(); break;
            case EnemyState.Wander: UpdateWander(); break;
            case EnemyState.Patrol: UpdatePatrol(); break;
            case EnemyState.Arrive: UpdateArrive(); break;
            case EnemyState.Attack: UpdateAttack(); break;
            case EnemyState.Cooldown: UpdateCooldown(); break;
            case EnemyState.Stun: UpdateStun(); break;
        }
    }

    // Metodos a sobre escribir
    protected virtual void UpdateIdle() { /* AGREGAR O SOBRE ESCRIBIR */ }
    protected virtual void UpdateSeek() 
    {
        // Si no hay un objetivo o fuera de rango (Seek -> Patrol)
        if (target == null ||
        Vector2.Distance(transform.position, target.transform.position) > detectionRadius + 1f)
        {
            target = null;
            currentState = EnemyState.Patrol;
            return;
        }
        // Si estamos cerca preparamos un pre ataque
        if (Vector2.Distance(transform.position, target.transform.position) <= attackRange + 1f)
        {
            timer = preAttackDuration;
            currentState = EnemyState.Arrive;
            return;
        }

        // Ejecutar seek
        Vector2 direction = (target.transform.position - transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);
    }
    protected virtual void UpdateFlee() { /* AGREGAR O SOBRE ESCRIBIR */ }
    protected virtual void UpdateWander()
    {
        // Si hay un objetivo (Wander -> Seek)
        if (DetectTarget())
        {
            currentState = EnemyState.Seek;
            return;
        }
        // ejecutar wander
        if (wanderDirection == Vector2.zero)
            wanderDirection = Random.insideUnitCircle.normalized;

        wanderDirection += Random.insideUnitCircle * wanderChange * Time.fixedDeltaTime;
        wanderDirection.Normalize();

        Vector2 forward = velocity.sqrMagnitude > 0.01f
            ? velocity.normalized
            : wanderDirection;

        Vector2 circleCenter = forward * wanderDistance;
        Vector2 wanderTarget = circleCenter + wanderDirection * wanderRadius;

        Vector2 desiredVelocity = wanderTarget.normalized * maxSpeed;

        Move(desiredVelocity);
    }
    protected virtual void UpdatePatrol()
    {
        // Si hay un objetivo (Patrol -> Seek)
        if (DetectTarget())
        {
            currentState = EnemyState.Seek;
            return;
        }
        // Si no hay minimo 2 puntos (Patrol -> Wander)
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            currentState = EnemyState.Wander;
            return;
        }

        Vector2 patrolPoint = patrolPoints[currentPatrolPoint];

        if (Vector2.Distance(transform.position, patrolPoint) <= patrolPointDistance)
        {
            currentPatrolPoint++;

            if (currentPatrolPoint >= patrolPoints.Length)
                currentPatrolPoint = 0;

            patrolPoint = patrolPoints[currentPatrolPoint];
        }

        Vector2 direction = (patrolPoint - (Vector2)transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;

        Move(desiredVelocity);
    }
    protected virtual void UpdateArrive() 
    {
        // Si no hay objetivo (Arrive -> Patrol)
        if (target == null)
        {
            currentState = EnemyState.Patrol;
            return;
        }
        timer -= Time.fixedDeltaTime;

        Vector2 direction = (target.transform.position - transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);

        // Si esta lista para atacar (Arrive -> Attack)
        if (timer <= 0f)
        {
            currentState = EnemyState.Attack;
        }
    }
    protected virtual void UpdateAttack() 
    {
        // Si no hay objetivo [fallo ataque] (Attack -> Cooldown)
        if (target == null)
        {
            currentState = EnemyState.Cooldown;
            timer = attackCooldown;
            return;
        }
        float distance = Vector2.Distance(transform.position, target.transform.position);
        if (distance <= attackRange) AttackTarget(target); // atacar objetivo 
        timer = attackCooldown;
        // Ataco al objetivo  (Attack -> Cooldown)
        currentState = EnemyState.Cooldown;
    }
    protected virtual void UpdateCooldown() 
    {
        timer -= Time.fixedDeltaTime;
        if (timer > 0f) return;
        // Despues del tiempo (Cooldown -> Seek)
        currentState = EnemyState.Seek; 
    }
    protected virtual void UpdateStun() { /* AGREGAR O SOBRE ESCRIBIR */ }
    // metodos de universales
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
    // Metodo de movimiento
    protected void Move(Vector2 desiredVelocity)
    {
        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, maxSpeed); // limitar velocidad deseada 

        velocity = Vector2.MoveTowards(
            velocity,
            desiredVelocity,
            acceleration * Time.fixedDeltaTime
        );
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
    // Metodo de deteccion
    protected bool DetectTarget()
    {
        Collider2D[] detectedTargets = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            npcLayer | playerLayer
        );

        EntityBase nearestPlayer = null;
        EntityBase nearestNPC = null;

        float playerDistance = Mathf.Infinity;
        float npcDistance = Mathf.Infinity;

        foreach (Collider2D collider in detectedTargets)
        {
            EntityBase detectedTarget = collider.GetComponentInParent<EntityBase>();

            if (detectedTarget == null || detectedTarget == this)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                detectedTarget.transform.position
            );

            if (((1 << detectedTarget.gameObject.layer) & playerLayer) != 0 &&
                distance < playerDistance)
            {
                nearestPlayer = detectedTarget;
                playerDistance = distance;
            }

            if (((1 << detectedTarget.gameObject.layer) & npcLayer) != 0 &&
                distance < npcDistance)
            {
                nearestNPC = detectedTarget;
                npcDistance = distance;
            }
        }

        EntityBase selectedTarget = null;

        switch (targetPriority)
        {
            case TargetPriority.Player:
                selectedTarget = nearestPlayer ?? nearestNPC;
                break;

            case TargetPriority.NPC:
                selectedTarget = nearestNPC ?? nearestPlayer;
                break;

            case TargetPriority.None:
                selectedTarget = playerDistance < npcDistance
                    ? nearestPlayer
                    : nearestNPC;
                break;
        }

        if (selectedTarget == null)
            return false;

        bool priorityTarget = targetPriority switch
        {
            TargetPriority.Player => ((1 << selectedTarget.gameObject.layer) & playerLayer) != 0,
            TargetPriority.NPC => ((1 << selectedTarget.gameObject.layer) & npcLayer) != 0,
            _ => false
        };

        if (target == null || changeTarget || priorityTarget)
            target = selectedTarget;

        return true;
    }

    // Modo Debug
    private void OnDrawGizmosSelected()
    {
        // RANGO DE DETECCION
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Rango para perder objetivo
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectionRadius + 1f);

        // RANGO DE ATAQUE
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Rango para iniciar preataque
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange + 1f);

        // OBJETIVO ACTUAL

        if (target != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                transform.position,
                target.transform.position
            );

            Gizmos.DrawWireSphere(
                target.transform.position,
                0.25f
            );
        }

        // DIRECCION / VELOCIDAD

        if (velocity.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawLine(
                transform.position,
                transform.position + (Vector3)velocity.normalized * 1.5f
            );
        }

        // WANDER

        if (currentState == EnemyState.Wander)
        {
            Vector2 forward = velocity.sqrMagnitude > 0.01f
                ? velocity.normalized
                : wanderDirection;

            Vector2 circleCenter = forward * wanderDistance;
            Vector2 wanderTarget = circleCenter + wanderDirection * wanderRadius;

            // Centro del círculo Wander
            Gizmos.color = Color.cyan;

            Gizmos.DrawWireSphere(
                transform.position + (Vector3)circleCenter,
                wanderRadius
            );

            // Dirección hacia el objetivo Wander
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                transform.position,
                transform.position + (Vector3)wanderTarget
            );

            // Punto objetivo Wander
            Gizmos.DrawWireSphere(
                transform.position + (Vector3)wanderTarget,
                0.2f
            );
        }

        // PATROL

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Gizmos.color = Color.cyan;

                Gizmos.DrawWireSphere(
                    patrolPoints[i],
                    0.25f
                );

                if (patrolPoints.Length > 1)
                {
                    int nextPoint = (i + 1) % patrolPoints.Length;

                    Gizmos.color = Color.blue;

                    Gizmos.DrawLine(
                        patrolPoints[i],
                        patrolPoints[nextPoint]
                    );
                }
            }

            if (currentState == EnemyState.Patrol &&
                currentPatrolPoint < patrolPoints.Length)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(
                    transform.position,
                    patrolPoints[currentPatrolPoint]
                );

                Gizmos.DrawWireSphere(
                    patrolPoints[currentPatrolPoint],
                    patrolPointDistance
                );
            }
        }

        // ARRIVE / PRE-ATAQUE

        if (currentState == EnemyState.Arrive && target != null)
        {
            Gizmos.color = Color.magenta;

            Gizmos.DrawLine(
                transform.position,
                target.transform.position
            );

            Gizmos.DrawWireSphere(
                transform.position,
                attackRange + 1f
            );
        }

        // FLEE

        if (currentState == EnemyState.Flee && target != null)
        {
            Vector2 fleeDirection =
                (transform.position - target.transform.position).normalized;

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(
                transform.position,
                transform.position + (Vector3)fleeDirection * 1.5f
            );
        }

        // ATTACK

        if (currentState == EnemyState.Attack && target != null)
        {
            Gizmos.color = Color.red;

            Gizmos.DrawLine(
                transform.position,
                target.transform.position
            );

            Gizmos.DrawWireSphere(
                target.transform.position,
                0.4f
            );
        }

        // COOLDOWN

        if (currentState == EnemyState.Cooldown)
        {
            Gizmos.color = Color.gray;

            Gizmos.DrawWireSphere(
                transform.position,
                0.4f
            );
        }
    }
}