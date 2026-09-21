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
        Die         // Muerte 
    }

    [Header("Configuracion IA")]
    public EnemyState currentState = EnemyState.Idle;
    public float moveSpeed = 3f;
    public int attackDamage = 1;
    [Header("Combate")]
    public float attackRange = 1.5f;
    public float preAttackTime = 0.4f;
    public float cooldownTime = 1.0f;
    [Header("Patrullaje (Wander)")]
    public Vector2[] patrolPoints;
    public float visionDistance = 5f;
    public LayerMask visionMask;
    
    [Header("Evasión")]
    public float obstacleCheck = 0.6f;

    protected EntityBase target;
    protected float timer = 0f;
    protected Rigidbody2D rb;
    protected int currentPointIndex = 0;
    protected Vector2 facingDirection = Vector2.down;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
    }
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
    protected virtual void UpdateIdle() { /* AGREGAR O SOBRE ESCRIBIR */ }
    protected virtual void UpdateWander() 
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, facingDirection, visionDistance, visionMask);

        if (hit.collider != null)
        {
            // Si el rayo choca con un Player o NPC
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("NPC"))
            {
                target = hit.collider.GetComponent<EntityBase>();
                if (target != null)
                {
                    currentState = EnemyState.Seek;
                    return;
                }
            }
        }

        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Vector2 targetPoint = patrolPoints[currentPointIndex];
        Vector2 offset = targetPoint - (Vector2)transform.position;

        if (Mathf.Abs(offset.x) < 0.1f && Mathf.Abs(offset.y) < 0.1f) // llegar punto
        {
            currentPointIndex++;
            if (currentPointIndex >= patrolPoints.Length) currentPointIndex = 0;
            return;
        }
        Vector2 direction = GetFourDirectionMovement(offset);
        rb.linearVelocity = direction * (moveSpeed * 0.75f);
    }
    protected virtual void UpdateSeek() 
    {
        if (target == null) return;
        Vector2 offset = target.transform.position - transform.position;
        float manhattanDistance = Mathf.Abs(offset.x) + Mathf.Abs(offset.y);

        // Seek -> PreAttack
        if (manhattanDistance <= attackRange)
        {
            timer = preAttackTime;
            currentState = EnemyState.PreAttack;
            return;
        }
        // Ejecutar seek
        Vector2 direction = GetFourDirectionMovement(offset);
        rb.linearVelocity = direction * moveSpeed;
    }
    protected virtual void UpdateFlee() { /* AGREGAR O SOBRE ESCRIBIR */ }
    protected virtual void UpdatePreAttack() 
    {
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 10f * Time.deltaTime);
        // PreAttack -> Attack
        timer -= Time.deltaTime;
        if (timer <= 0f) currentState = EnemyState.Attack;
    }
    protected virtual void UpdateAttack() 
    {
        if (target == null)
        {
            currentState = EnemyState.Idle; // Attack -> Idle
            return;
        }
        float distanceX = Mathf.Abs(transform.position.x - target.transform.position.x);
        float distanceY = Mathf.Abs(transform.position.y - target.transform.position.y);
        float manhattanDistance = distanceX + distanceY;
        if (manhattanDistance <= attackRange + 0.2f) // margen
        {
            AttackTarget(target);
        }
        // Attack -> Cooldown
        timer = cooldownTime;
        currentState = EnemyState.Cooldown;
    }
    protected virtual void UpdateCooldown()
    {
        //  Cooldown -> Seek
        timer -= Time.deltaTime;
        if (timer <= 0f) currentState = EnemyState.Seek;
    }
    protected virtual void UpdateStun() { /* AGREGAR O SOBRE ESCRIBIR */ }
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
    // extras 
    protected Vector2 GetFourDirectionMovement(Vector2 offset)
    {
        // Las 4 direcciones posibles
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        Vector2 bestDirection = Vector2.zero;
        float shortestDistance = float.MaxValue;

        // Las coordenadas exactas del objetivo
        Vector2 targetPoint = (Vector2)transform.position + offset;
        bool foundPath = false;

        foreach (Vector2 dir in directions)
        {
            // 1. Jamas dar la vuelta en "U" (evita que tiemble arriba y abajo)
            if (facingDirection != Vector2.zero && dir == -facingDirection)
                continue;

            // 2. No intentar atravesar paredes
            if (IsWall(dir))
                continue;

            // 3. Proyectar un paso imaginario en esa direccion y medir que tan cerca nos deja de la meta
            Vector2 imaginaryPosition = (Vector2)transform.position + (dir * 1f);
            float realDistance = Vector2.Distance(imaginaryPosition, targetPoint);

            if (facingDirection != Vector2.zero && dir != facingDirection)
            {
                realDistance += 0.5f;
            }
            if (realDistance < shortestDistance)
            {
                shortestDistance = realDistance;
                bestDirection = dir;
                foundPath = true;
            }
        }

        // 4. Si todas las direcciones estan tapadas (un callejon sin salida)
        // Rompemos la regla y le permitimos dar la vuelta en "U" para salir.
        if (!foundPath)
        {
            if (!IsWall(-facingDirection))
            {
                bestDirection = -facingDirection;
            }
        }

        // Guardar hacia donde estamos mirando para el proximo fotograma
        if (bestDirection != Vector2.zero)
        {
            facingDirection = bestDirection;
        }

        return bestDirection;
    }

    protected bool IsWall(Vector2 dir)
    {
        Vector2 origin = transform.position;

        if (dir != facingDirection && facingDirection != Vector2.zero)
        {
            origin = (Vector2)transform.position - (facingDirection * 0.6f);
        }

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, obstacleCheck, visionMask);

        if (hit.collider != null && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("NPC"))
        {
            return true;
        }
        return false;
    }
    private void OnDrawGizmosSelected()
    {
        // 1. Dibujar los puntos de patrulla
        if (patrolPoints != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector2 punto in patrolPoints)
            {
                Gizmos.DrawWireSphere(punto, 0.3f);
            }
        }

        // 2. Dibujar el rayo de visión
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, facingDirection * visionDistance);
    }
}