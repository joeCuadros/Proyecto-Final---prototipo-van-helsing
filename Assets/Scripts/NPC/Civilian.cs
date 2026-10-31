using UnityEngine;

public class Civilian : AIBase
{
    public enum FearState
    {
        None,       // Estado inicial para forzar la primera evaluación
        Tranquilo,  // 00 - 25
        Asustado,   // 25 - 50
        Alterado,   // 50 - 75
        Panico      // 75 - 100
    }

    [Header("Sistema de Miedo")]
    [Range(0f, 100f)] public float fearLevel = 0f;
    public float fearIncreaseRate = 15f; // (+) Sube con enemigos cerca
    public float fearDecreaseRate = 10f; // (-) Baja con Van Helsing cerca
    public float timeFearDecayRate = 2f; // (-) Baja poco a poco con el tiempo

    [Header("Condicion del Civil")]
    public bool isInjured = false; // Esta herido
    [Range(0, 100)] public int conversionProbability = 10; // Probabilidad

    private float baseMaxSpeed;
    private FearState lastFearState = FearState.None;

    private EntityBase nearestPlayer;
    private EntityBase nearestEnemy;

    protected override void Start()
    {
        enemyType = "Civilian";
        baseMaxSpeed = maxSpeed;
        base.Start();
    }

    protected override void FixedUpdate()
    {
        if (currentState == EnemyState.Die || currentState == EnemyState.Stun) return;
        UpdateFearAndState();
        base.FixedUpdate(); // Ejecutar el cerebro AIBase
    }

    private void UpdateFearAndState()
    {
        // 1. Escanear el entorno con nuestro DetectTarget reescrito
        DetectTarget();

        // 2. Actualizar valor de miedo (0 - 100)
        if (nearestEnemy != null) fearLevel += fearIncreaseRate * Time.fixedDeltaTime;
        if (nearestPlayer != null) fearLevel -= fearDecreaseRate * Time.fixedDeltaTime;
        fearLevel -= timeFearDecayRate * Time.fixedDeltaTime;
        fearLevel = Mathf.Clamp(fearLevel, 0f, 100f);

        // 3. Calcular en qué rango de miedo se encuentra
        FearState currentFearState;
        if (fearLevel <= 25f) currentFearState = FearState.Tranquilo;
        else if (fearLevel <= 50f) currentFearState = FearState.Asustado;
        else if (fearLevel <= 75f) currentFearState = FearState.Alterado;
        else currentFearState = FearState.Panico;

        // 4. Aplicar cambios de estado y velocidad SOLO si cambió el estado de miedo
        if (currentFearState != lastFearState)
        {
            lastFearState = currentFearState;

            switch (currentFearState)
            {
                case FearState.Tranquilo: // 00-25 -> Seek
                    currentState = EnemyState.Seek;
                    maxSpeed = baseMaxSpeed * 1.0f;
                    target = nearestPlayer;
                    break;

                case FearState.Asustado: // 25-50 -> Seek lento
                    currentState = EnemyState.Seek;
                    maxSpeed = baseMaxSpeed * 0.5f;
                    target = nearestPlayer;
                    break;

                case FearState.Alterado: // 50-75 -> Idle (bloqueado)
                    currentState = EnemyState.Idle;
                    maxSpeed = 0f;
                    target = null;
                    break;

                case FearState.Panico: // 75-100 -> Flee (limitado a 1.0 si está herido)
                    currentState = EnemyState.Flee;
                    maxSpeed = baseMaxSpeed * (isInjured ? 1.0f : 1.5f);
                    target = nearestEnemy;
                    break;
            }
        }
    }

    protected override bool DetectTarget()
    {
        Collider2D[] detectedTargets = Physics2D.OverlapCircleAll(
            transform.position,
            detectionRadius,
            enemyLayer | playerLayer
        );

        nearestPlayer = null;
        nearestEnemy = null;

        float playerDistance = Mathf.Infinity;
        float enemyDistance = Mathf.Infinity;

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

            if (((1 << detectedTarget.gameObject.layer) & enemyLayer) != 0 &&
                distance < enemyDistance)
            {
                nearestEnemy = detectedTarget;
                enemyDistance = distance;
            }
        }

        // Seleccionar el objetivo según el estado de miedo actual
        EntityBase selectedTarget = lastFearState switch
        {
            FearState.Tranquilo or FearState.Asustado => nearestPlayer,
            FearState.Panico => nearestEnemy,
            _ => null
        };

        if (selectedTarget == null)
        {
            target = null;
            return false;
        }

        target = selectedTarget;
        return true;
    }

    protected override void UpdateIdle()
    {
        // En Alterado se queda quieto bloqueado hasta cambiar de estado de miedo
        Move(Vector2.zero);
    }

    protected override void UpdateWander()
    {
        // Si mientras deambula detecta objetivo, pasa a Flee (en Panico) o a Seek (Tranquilo/Asustado)
        if (DetectTarget())
        {
            currentState = (lastFearState == FearState.Panico)
                ? EnemyState.Flee
                : EnemyState.Seek;
            return;
        }

        base.UpdateWander();
    }

    protected override void UpdateSeek()
    {
        // Si no hay un objetivo o está fuera de rango (Seek -> Wander)
        if (target == null ||
            Vector2.Distance(transform.position, target.transform.position) > detectionRadius + 1f)
        {
            target = null;
            currentState = EnemyState.Wander;
            return;
        }

        // Ejecutar Seek
        Vector2 direction = (target.transform.position - transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);
    }

    protected override void UpdateFlee()
    {
        // Si no hay un objetivo o ya está fuera del rango seguro (Flee -> Wander)
        if (target == null ||
            Vector2.Distance(transform.position, target.transform.position) > detectionRadius + 2f)
        {
            target = null;
            currentState = EnemyState.Wander;
            return;
        }

        // Ejecutar Flee
        Vector2 direction = (transform.position - target.transform.position).normalized;
        Vector2 desiredVelocity = direction * maxSpeed;
        Move(desiredVelocity);
    }
}