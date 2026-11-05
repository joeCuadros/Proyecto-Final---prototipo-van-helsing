using UnityEngine;

// Solo consulta el siguiente waypoint de Dijkstra.
// NO mueve el Rigidbody2D: el unico controlador es Civilian.
[RequireComponent(typeof(Civilian))]
[RequireComponent(typeof(Rigidbody2D))]
public class CivilDijkstraNavigator : MonoBehaviour
{
    [Header("Navegacion")]
    [SerializeField] private DijkstraGrid2D grid;

    [Header("Diagnostico de colisiones")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Depuracion")]
    [SerializeField] private bool showRoute = true;

    private Civilian civilian;

    [Header("Precision de ruta")]
    [SerializeField] private float waypointReachedDistance = 0.20f;

    private Vector2 currentWaypoint;
    private bool hasWaypoint = false;

    private void Awake()
    {
        civilian = GetComponent<Civilian>();
    }

    private void Start()
    {
        if (grid == null)
            grid = FindFirstObjectByType<DijkstraGrid2D>();
    }

    public bool CanNavigate =>
        civilian != null &&
        grid != null &&
        grid.IsReady &&
        !civilian.isUnconscious &&
        civilian.EscapeMode == Civilian.CivilEscapeMode.Independent;



    public bool TryGetWaypoint(out Vector2 waypoint)
    {
        waypoint = transform.position;

        if (!CanNavigate)
            return false;

        // Consultar la ruta según la posición actual.
        // No conservar indefinidamente un punto inaccesible.
        return grid.TryGetNextWaypoint(
            transform.position,
            out waypoint
        );
    }



    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !showRoute)
            return;

        if (!TryGetWaypoint(out Vector2 waypoint))
            return;

        Vector2 origin = transform.position;
        Vector2 direction = waypoint - origin;

        RaycastHit2D hit = Physics2D.Linecast(
            origin,
            waypoint,
            obstacleLayer
        );

        if (hit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, waypoint);
            Gizmos.DrawWireSphere(waypoint, 0.15f);
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, waypoint);
        Gizmos.DrawSphere(waypoint, 0.08f);
    }
}
