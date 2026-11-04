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
    }

    protected override void FixedUpdate()
    {
        if (currentState == EnemyState.Die || currentState == EnemyState.Stun) return;
        UpdateFearAndState();
        base.FixedUpdate(); // Ejecutar el cerebro AIBase
    }


    private void UpdateFearAndState()
    {
        DetectTarget();


        float fearPerSecond = CalculateEnemyFearRate();

        fearLevel += fearPerSecond * Time.fixedDeltaTime;

        fearLevel = Mathf.Clamp(fearLevel, 0f, 100f);


        if (isUnconscious)
            return;

        // Se comprueba el desmayo al alcanzar miedo máximo.
        if (fearLevel >= 100f)
        {
            TryFaint();

            if (isUnconscious)
                return;
        }

        // Desde 60 % se inmoviliza y se protege.
        if (fearLevel >= 60f)
        {
            ActivateSalt();
            return;
        }

        // Por debajo de 60 %, puede desplazarse.
        if (isSaltProtected)
            DeactivateSalt();

        FearState newState =
            fearLevel <= 25f
            ? FearState.Tranquilo
            : FearState.Asustado;

        if (newState != lastFearState ||
            currentState == EnemyState.Idle)
        {
            lastFearState = newState;

            maxSpeed = newState == FearState.Tranquilo
                ? baseMaxSpeed
                : baseMaxSpeed * 0.75f;

            target = nearestPlayer;

            currentState = target != null
                ? EnemyState.Seek
                : EnemyState.Wander;
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

    private void ActivateSalt()
    {
        if (isUnconscious)
            return;

        isSaltProtected = true;
        currentState = EnemyState.Idle;
        maxSpeed = 0f;
        target = null;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

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
        if (isUnconscious ||
            currentState == EnemyState.Die)
            return;

        fearLevel = 0f;
        DeactivateSalt();

        maxSpeed = baseMaxSpeed;
        lastFearState = FearState.None;
        currentState = EnemyState.Wander;

        Debug.Log("Civil ayudado: miedo reiniciado a 0.");
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
                rb.linearVelocity = Vector2.zero;

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
        base.TakeDamage(damage);
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