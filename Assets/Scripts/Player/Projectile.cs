using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [Header("Configuracion")]
    public float speed = 10f;
    public float maxDistance = 8f;
    public int damage = 1;
    public WeaponType weapon;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Vector2 startPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, int projectileDamage)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        damage = projectileDamage;
        startPosition = rb.position;
        rb.linearVelocity = direction.normalized * speed;
    }

    void FixedUpdate()
    {
        if (Vector2.Distance(startPosition, rb.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            EntityBase enemy = other.GetComponentInParent<EntityBase>();
            if (enemy != null) enemy.TakeDamage(damage, weapon);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
            Destroy(gameObject);
    }
}