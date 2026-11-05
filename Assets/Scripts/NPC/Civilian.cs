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


    public enum CivilEscapeMode
    {
        Unassisted,        // Sin ayuda: huye o busca a Van Helsing
        Independent,       // Ayudado: va solo usando Dijkstra
        FollowingPlayer,   // Ayudado: debe seguir a Van Helsing
        SaltProtected,     // Paralizado dentro del círculo
        Unconscious,       // Desmayado
        Rescued            // Llegó a la zona segura
    }

    [Header("Voces")]
    public AudioClip hurtClip;
    public AudioClip helpClip;
    
    [Header("Estado del escape")]
    [SerializeField] private CivilEscapeMode escapeMode = CivilEscapeMode.Unassisted;
    public CivilEscapeMode EscapeMode => escapeMode;
    public bool HasReceivedHelp => hasReceivedHelp;
    private bool hasReceivedHelp;

    [Header("Van Helsing / seguimiento")]
    [SerializeField] private float safetyRadius = 2.5f;
    [SerializeField] private float followingStopDistance = 1.1f;
    [SerializeField] private float followingLostDistance = 4f;
    private Transform followingPlayer;

    private CivilDijkstraNavigator navigator;
    private bool warnedMissingRoute;

    [Header("Movimiento Dijkstra")]
    [SerializeField] private float navigationSpeed = 1.5f;
    [SerializeField] private float slowDownDistance = 0.65f;

    [Header("Sistema de Miedo")]
    [Range(0f, 100f)] public float fearLevel = 0f;
    public float fearIncreaseRate = 1f;
    public float fearDecreaseRate = 0f;
    public float timeFearDecayRate = 0f;

    [Header("Proteccion de sal")]
    public GameObject saltCircleVisual;

    public bool isUnconscious = false;

    private bool isSaltProtected = false;
    private bool faintChecked = false;
    public bool IsSaltProtected =>
    isSaltProtected &&
    !isUnconscious &&
    currentState != EnemyState.Die;

    [Header("Condicion del Civil")]
    public bool isInjured = false; // Esta herido
    [Range(0, 100)] public int conversionProbability = 10; // Probabilidad

    private float baseMaxSpeed;
    private FearState lastFearState = FearState.None;

    private EntityBase nearestPlayer;
    private EntityBase nearestEnemy;

    [Header("Miedo segun tipo de enemigo")]
    public float vampireFearPer10Seconds = 5f;
    public float werewolfFearPer10Seconds = 7.5f;
    public float demonFearPer10Seconds = 10f;


    protected override void Start()
    {
        enemyType = "Civilian";
        baseMaxSpeed = maxSpeed;
        base.Start();
        navigator = GetComponent<CivilDijkstraNavigator>();

        if (saltCircleVisual != null)
            saltCircleVisual.SetActive(false);

        // Prefabs viejos pueden haber guardado un estado distinto.
        escapeMode = CivilEscapeMode.Unassisted;
    }

    protected override void FixedUpdate()
    {
        if (currentState == EnemyState.Die || isUnconscious)
            return;

        UpdateFearAndState();

        if (isUnconscious)
            return;

        switch (escapeMode)
        {
            case CivilEscapeMode.Unassisted:
                // Antes de recibir ayuda huye de monstruos, se acerca
                // a Van Helsing si lo ve, o deambula.
                maxSpeed = baseMaxSpeed;
                if (nearestEnemy != null)
                {
                    target = nearestEnemy;
                    currentState = EnemyState.Flee;
                }
                else if (nearestPlayer != null)
                {
                    target = nearestPlayer;
                    currentState = EnemyState.Seek;
                }
                else
                {
                    target = null;
                    currentState = EnemyState.Wander;
                }
                base.FixedUpdate();
                break;

            case CivilEscapeMode.Independent:
                currentState = EnemyState.Idle;
                MoveToSafeZone();
                break;

            case CivilEscapeMode.FollowingPlayer:
                currentState = EnemyState.Idle;
                FollowVanHelsing();
                break;

            case CivilEscapeMode.SaltProtected:
                currentState = EnemyState.Idle;
                StopMoving();
                break;

            case CivilEscapeMode.Unconscious:
            case CivilEscapeMode.Rescued:
                StopMoving();
                break;
        }
    }

    private void UpdateFearAndState()
    {
        DetectTarget();

        // La proximidad de Van Helsing detiene el aumento, no lo reduce.
        float rate = IsNearVanHelsing() ? 0f : CalculateEnemyFearRate();
        fearLevel = Mathf.Clamp(fearLevel + rate * Time.fixedDeltaTime, 0f, 100f);

        if (fearLevel >= 100f)
        {
            TryFaint();
            if (isUnconscious)
                return;
        }

        if (escapeMode == CivilEscapeMode.SaltProtected)
            return; // Nunca sale del circulo por ver al jugador.

        if (escapeMode == CivilEscapeMode.Unassisted && fearLevel >= 60f)
        {
            ActivateSalt();
            return;
        }

        if (escapeMode == CivilEscapeMode.Independent && fearLevel >= 60f)
        {
            ActivateSalt();
            return;
        }

        // FollowingPlayer puede tener 60-100 %: sigue al jugador
        // mientras no lo pierda. El alejamiento se comprueba aparte.
    }

    private bool IsNearVanHelsing()
    {
        // nearestPlayer se calcula con deteccion y linea de vision.
        return nearestPlayer != null &&
            Vector2.Distance(transform.position, nearestPlayer.transform.position)
                <= safetyRadius;
    }

    private void MoveToSafeZone()
    {
        target = null;
        maxSpeed = baseMaxSpeed;
        if (navigator == null || !navigator.TryGetWaypoint(out Vector2 waypoint))
        {
            StopMoving();
            if (!warnedMissingRoute)
            {

                DijkstraGrid2D dijkstraGrid =
                    FindFirstObjectByType<DijkstraGrid2D>();

                Debug.LogWarning(
                    $"{name}: sin ruta Dijkstra. " +
                    (dijkstraGrid != null
                        ? dijkstraGrid.ExplainRouteFailure(transform.position)
                        : "No existe DijkstraGrid2D en la escena.")
                );

            }
            return;
        }


        warnedMissingRoute = false;

        Vector2 direction = waypoint - rb.position;
        float distance = direction.magnitude;

        // Reducir la velocidad al acercarse al waypoint.

        float speedFactor = Mathf.Clamp(
            distance / Mathf.Max(0.01f, slowDownDistance),
            0.65f,
            1f
        );

        float desiredSpeed = navigationSpeed * speedFactor;


        // Limitar la velocidad del civil mientras navega.
        maxSpeed = navigationSpeed;

        // Si está extremadamente cerca, frenar.
        if (distance < 0.05f)
        {
            Move(Vector2.zero);
            return;
        }

        Move(direction.normalized * desiredSpeed);

    }

    private void FollowVanHelsing()
    {
        target = null;
        maxSpeed = baseMaxSpeed;

        if (followingPlayer == null)
        {
            ActivateSalt();
            return;
        }

        Vector2 offset = (Vector2)followingPlayer.position - rb.position;
        float distance = offset.magnitude;

        if (distance > followingLostDistance)
        {
            ActivateSalt();
            return;
        }

        if (distance <= followingStopDistance)
            Move(Vector2.zero);
        else
            Move(offset.normalized * maxSpeed);
    }

    private void StopMoving()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        velocity = Vector2.zero;
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

        return nearestEnemy != null || nearestPlayer != null;
    }

    protected override void UpdateIdle()
    {
        // En Alterado se queda quieto bloqueado hasta cambiar de estado de miedo
        Move(Vector2.zero);
    }

    protected override void UpdateWander()
    {
        // Se usa solo antes de recibir ayuda. FixedUpdate selecciona
        // Seek/Flee cada frame si aparece jugador o monstruo.
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

        if (Vector2.Distance(transform.position, target.transform.position) <= followingStopDistance)
        {
            Move(Vector2.zero);
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

    private void ActivateSalt()
    {
        if (isUnconscious)
            return;

        escapeMode = CivilEscapeMode.SaltProtected;
        isSaltProtected = true;
        currentState = EnemyState.Idle;
        maxSpeed = 0f;
        target = null;
        StopMoving();

        if (saltCircleVisual != null)
            saltCircleVisual.SetActive(true);
    }

    private void DeactivateSalt()
    {
        isSaltProtected = false;
        if (saltCircleVisual != null)
            saltCircleVisual.SetActive(false);
    }

    public void HelpByVanHelsing()
    {
        if (isUnconscious || currentState == EnemyState.Die ||
            escapeMode == CivilEscapeMode.Rescued)
            return;

        // El miedo NO disminuye nunca.
        bool firstHelp = !hasReceivedHelp;
        hasReceivedHelp = true;
        if (nearestPlayer != null)
            followingPlayer = nearestPlayer.transform;

        DeactivateSalt();
        maxSpeed = baseMaxSpeed;
        target = null;
        currentState = EnemyState.Idle;
        warnedMissingRoute = false;

        escapeMode = fearLevel < 60f
            ? CivilEscapeMode.Independent
            : CivilEscapeMode.FollowingPlayer;

        Debug.Log($"{name}: ayuda recibida; miedo {fearLevel:0.0}%; " +
            $"modo {escapeMode}.");

        if (firstHelp && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(helpClip);
    }

    private void TryFaint()
    {
        if (faintChecked || isUnconscious)
            return;

        faintChecked = true;

        // Probabilidad del 10 %, evaluada una sola vez.

        if (Random.value <= 0.10f)
        {
            isUnconscious = true;
            DeactivateSalt();

            currentState = EnemyState.Stun;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                // Evita que la física lo desplace.
                rb.constraints =
                    RigidbodyConstraints2D.FreezeAll;
            }

            Debug.Log("El civil se ha desmayado.");
        }

    }

    public override void TakeDamage(int damage)
    {
        // Si está protegido por sal, no recibe daño.
        if (IsSaltProtected)
        {
            Debug.Log(
                $"{gameObject.name} está protegido por sal."
            );

            return;
        }

        // Si está desmayado o sin protección,
        // recibe el daño normalmente.
        int previousHealth = CurrentHealth;

        base.TakeDamage(damage);

        if (CurrentHealth < previousHealth &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hurtClip);
        }
    }

    public override void TakeDamage(
        int damage,
        WeaponType weapon)
    {
        // Reutilizar la misma comprobación
        // para ataques con armas o proyectiles.
        TakeDamage(damage);
    }


    private float CalculateEnemyFearRate()
    {
        Collider2D[] nearbyColliders =
            Physics2D.OverlapCircleAll(
                transform.position,
                detectionRadius,
                enemyLayer
            );

        float highestFearRate = 0f;

        foreach (Collider2D hit in nearbyColliders)
        {
            AIBase enemy = hit.GetComponentInParent<AIBase>();

            if (enemy == null || enemy == this)
                continue;

            if (enemy.currentState == EnemyState.Die)
                continue;

            // Comprobar si existe una pared entre ambos.
            RaycastHit2D wall = Physics2D.Linecast(
                transform.position,
                enemy.transform.position,
                LayerMask.GetMask("StealthObstacle")
            );

            if (wall.collider != null)
                continue;

            float fearRate = 0f;

            if (enemy is EnemyDemon)
            {
                fearRate = demonFearPer10Seconds;
            }
            else if (enemy is EnemyWerewolf)
            {
                fearRate = werewolfFearPer10Seconds;
            }
            else if (enemy is EnemyVampire)
            {
                fearRate = vampireFearPer10Seconds;
            }

            highestFearRate = Mathf.Max(
                highestFearRate,
                fearRate
            );
        }

        // Convertir porcentaje por 10 segundos
        // a porcentaje por segundo.
        return highestFearRate / 10f;
    }


}