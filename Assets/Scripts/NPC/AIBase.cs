using UnityEngine;

public abstract class AIBase : EntityBase
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
        Investigate,// Investigar el punto solicitado
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
    public float attackRange = 1f;
    public float preAttackDuration = 1f;
    public float attackCooldown = 1f;

    [Header("Wander")]
    public float wanderRadius = 2f;
    public float wanderDistance = 2f;
    public float wanderChange = 10f;
    protected Vector2 wanderDirection; // no publico

    [Header("Waypoints")]
    public Vector2[] waypoints;
    public float waypointDistance = 0.2f;
    protected int currentWaypointIndex = 0; // no publico

    [Header("Movimiento")]
    public float maxSpeed = 3f;
    public float acceleration = 4f;
    public float deceleration = 6f;
    protected Vector2 velocity; // no publico

    [Header("Deteccion")]
    public float detectionRadius = 4f;
    public LayerMask npcLayer;
    public LayerMask playerLayer;
    public LayerMask enemyLayer;
    public TargetPriority targetPriority;
    public bool changeTarget = true;
    protected EntityBase target; // no publico

    [Header("Investigar")]
    public Vector2 investigatePoint;

    // GENERALES
    protected Rigidbody2D rb;
    private Collider2D col;
    protected float timer = 0f;
    protected string enemyType = "BaseEnemy";
    private int avoidSide = 0;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        if (!(this is Civilian) && LevelManager.Instance != null)
        {
            LevelManager.Instance.RegisterEnemy(enemyType, this);
        }
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
            case EnemyState.Investigate: UpdateInvestigate(); break;
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
        if (Vector2.Distance(transform.position, target.transform.position) <= attackRange + 0.5f)
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
    protected virtual void UpdateFlee() 
    {
        // Si no hay un objetivo o ya está fuera del rango seguro (Flee -> Patrol)
        if (target == null ||
            Vector2.Distance(transform.position, target.transform.position) > detectionRadius + 2f)
        {
            target = null;
            currentState = EnemyState.Patrol;
            return;
        }

        // Ejecutar Flee
        Vector2 direction = (transform.position - target.transform.position).normalized; // opuesto a seek
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);
    }
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
        if (waypoints == null || waypoints.Length < 2)
        {
            currentState = EnemyState.Wander;
            return;
        }

        Vector2 patrolPoint = waypoints[currentWaypointIndex];

        if (Vector2.Distance(transform.position, patrolPoint) <= waypointDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
                currentWaypointIndex = 0;

            patrolPoint = waypoints[currentWaypointIndex];
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
    protected virtual void UpdateInvestigate() {
        // Si hay un objetivo (Investigate -> Seek)
        if (DetectTarget())
        {
            currentState = EnemyState.Seek;
            return;
        }
        // Si llego al punto (Investigate -> Patrol)
        if (Vector2.Distance(transform.position, investigatePoint) <= waypointDistance)
        {
            currentState = EnemyState.Patrol;
            return;
        }
        Vector2 direction = (investigatePoint - (Vector2)transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);
    }
    // metodos de universales
    public virtual void AttackTarget(EntityBase target)
    {
        target.TakeDamage(attackDamage);
    }
    protected override void Die()
    {
        currentState = EnemyState.Die;
        if (!(this is Civilian) && LevelManager.Instance != null)
        {
            LevelManager.Instance.EnemyDied(enemyType, this);
        }
        Destroy(gameObject, 0.5f);
    }
    // Metodo de movimiento
    protected void Move(Vector2 desiredVelocity)
    {
        desiredVelocity = Vector2.ClampMagnitude(desiredVelocity, maxSpeed);

        // ATASCO: comparo lo ordenado (velocity) con la velocidad real de la fisica
        bool nearTarget = target != null &&
            Vector2.Distance(rb.position, target.transform.position) <= attackRange + 0.5f;
        bool stuck = !nearTarget &&
            velocity.magnitude > maxSpeed * 0.5f &&
            rb.linearVelocity.magnitude < velocity.magnitude * 0.3f;

        if (desiredVelocity.sqrMagnitude > 0.0001f)
        {
            int mask = LayerMask.GetMask("StealthObstacle");
            float sensorLength = 0.15f;
            float step = 45f;
            Vector2 origin = col.bounds.center;       // centro del collider, no del sprite
            Vector2 size = col.bounds.size * 0.9f;    // un poco mas chica para no empezar tocando la pared
            Vector2 dir = desiredVelocity.normalized;

            bool Blocked(float angle) =>
                Physics2D.BoxCast(
                    origin,
                    size,
                    0f,
                    (Vector2)(Quaternion.Euler(0f, 0f, angle) * dir),
                    sensorLength,
                    mask
                ).collider != null;

            // primer angulo libre hacia un lado (999 = ninguno)
            float FirstFree(int side, float start)
            {
                for (float a = start; a <= 180f; a += step)
                    if (!Blocked(a * side)) return a;
                return 999f;
            }

            bool blocked = Blocked(0f) || Blocked(step) || Blocked(-step);

            // soltar la pared: camino libre Y ya no hay pared a su lado
            if (avoidSide != 0 && !blocked && !Blocked(-avoidSide * 90f))
                avoidSide = 0;

            // elegir lado una sola vez: el que necesite girar menos
            if (avoidSide == 0 && blocked)
            {
                float left = FirstFree(1, step);
                float right = FirstFree(-1, step);
                avoidSide = left < right ? 1 : right < left ? -1 : (Random.value < 0.5f ? 1 : -1);
            }

            // siguiendo la pared: girar solo lo minimo necesario
            if (avoidSide != 0)
            {
                float angle = FirstFree(avoidSide, 0f);
                if (angle < 999f)
                    dir = (Vector2)(Quaternion.Euler(0f, 0f, angle * avoidSide) * dir);

                desiredVelocity = dir * desiredVelocity.magnitude;
            }
        }

        velocity = rb.linearVelocity;

        if (stuck && desiredVelocity.sqrMagnitude > 0.0001f)
        {
            Vector2 stuckDir = desiredVelocity.normalized;
            RaycastHit2D wall = Physics2D.BoxCast(
                col.bounds.center,
                col.bounds.size * 0.9f,
                0f,
                stuckDir,
                0.3f,
                LayerMask.GetMask("StealthObstacle")
            );

            if (wall.collider != null)
            {
                Vector2 slide = stuckDir - wall.normal * Vector2.Dot(stuckDir, wall.normal);
                Vector2 safeDir = (slide.normalized + wall.normal).normalized;
                velocity = safeDir * maxSpeed;
            }
            else
            {
                velocity = desiredVelocity;
            }
        }
        else
        {
            velocity = Vector2.MoveTowards(
                velocity,
                desiredVelocity,
                acceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = velocity;
    }
    // Metodo de deteccion
    protected virtual bool DetectTarget()
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

            RaycastHit2D hit = Physics2D.Linecast(
                transform.position,
                detectedTarget.transform.position,
                LayerMask.GetMask("StealthObstacle")
            );

            if (hit.collider != null)
            {
                continue;
            }


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
        Gizmos.DrawWireSphere(transform.position, attackRange + 0.5f);

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

        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.color = Color.cyan;

                Gizmos.DrawWireSphere(
                    waypoints[i],
                    0.25f
                );

                if (waypoints.Length > 1)
                {
                    int nextPoint = (i + 1) % waypoints.Length;

                    Gizmos.color = Color.blue;

                    Gizmos.DrawLine(
                        waypoints[i],
                        waypoints[nextPoint]
                    );
                }
            }

            if (currentState == EnemyState.Patrol &&
                currentWaypointIndex < waypoints.Length)
            {
                Gizmos.color = Color.green;

                Gizmos.DrawLine(
                    transform.position,
                    waypoints[currentWaypointIndex]
                );

                Gizmos.DrawWireSphere(
                    waypoints[currentWaypointIndex],
                    waypointDistance
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
        // investigate
        if (currentState == EnemyState.Investigate)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(
                transform.position,
                investigatePoint
            );

            Gizmos.DrawWireSphere(
                investigatePoint,
                waypointDistance
            );
        }
    }
}