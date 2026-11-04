
using System.Collections.Generic;
using UnityEngine;

public class DijkstraGrid2D : MonoBehaviour
{
    [Header("Mapa")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 40;
    [SerializeField] private float cellSize = 0.5f;

    [Header("Colisiones")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float collisionPadding = 0.05f;

    [Header("Destino")]
    [SerializeField] private Collider2D safeZone;

    [Header("Depuracion")]
    [SerializeField] private bool showGrid = true;

    [Header("Margen de seguridad")]
    [SerializeField] private float wallClearance = 0.08f;

    private bool[] blocked;
    private float[] distance;
    private int[] nextCell;
    private bool ready;

    private static readonly Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    public bool IsReady => ready;

    private void Start()
    {
        BuildGrid();
    }

    public void BuildGrid()
    {
        ready = false;

        if (width <= 0 || height <= 0 || cellSize <= 0f)
        {
            Debug.LogError("Configuracion de grid invalida.");
            return;
        }

        if (safeZone == null)
        {
            Debug.LogError(
                "Asigna el Collider2D de ZonaSegura."
            );
            return;
        }

        int count = width * height;

        blocked = new bool[count];
        distance = new float[count];
        nextCell = new int[count];


        Vector2 checkSize = Vector2.one *
            Mathf.Max(
                0.01f,
                cellSize - collisionPadding
                + wallClearance * 2f
            );


        for (int i = 0; i < count; i++)
        {
            Vector2 center = CellCenter(i);

            blocked[i] = Physics2D.OverlapBox(
                center,
                checkSize,
                0f,
                obstacleLayer
            ) != null;

            distance[i] = float.PositiveInfinity;
            nextCell[i] = -1;
        }

        Vector2 destination = safeZone.bounds.center;

        int goal = FindClosestWalkable(destination);

        if (goal == -1)
        {
            Debug.LogError(
                "No hay una celda transitable para el destino."
            );
            return;
        }

        // Cola de prioridad ordenada por costo.
        var queue =
            new SortedSet<(float cost, long order, int index)>();

        long order = 0;
        distance[goal] = 0f;
        queue.Add((0f, order++, goal));

        while (queue.Count > 0)
        {
            var current = queue.Min;
            queue.Remove(current);

            int index = current.index;

            if (current.cost > distance[index])
                continue;

            int x = index % width;
            int y = index / width;

            foreach (Vector2Int direction in directions)
            {
                int nx = x + direction.x;
                int ny = y + direction.y;

                if (nx < 0 || nx >= width ||
                    ny < 0 || ny >= height)
                    continue;

                int neighbor = ny * width + nx;

                if (blocked[neighbor])
                    continue;

                // Todas las conexiones tienen costo 1.
                float candidate = distance[index] + 1f;

                if (candidate >= distance[neighbor])
                    continue;

                distance[neighbor] = candidate;

                // Desde neighbor, avanzar hacia index.
                nextCell[neighbor] = index;

                queue.Add((candidate, order++, neighbor));
            }
        }

        ready = true;
        Debug.Log("Dijkstra calculado correctamente.");
    }

    private Vector2 CellCenter(int index)
    {
        int x = index % width;
        int y = index / width;

        return (Vector2)transform.position +
            new Vector2(
                (x + 0.5f) * cellSize,
                (y + 0.5f) * cellSize
            );
    }

    private int WorldToIndex(Vector2 position)
    {
        Vector2 local =
            position - (Vector2)transform.position;

        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);

        if (x < 0 || x >= width ||
            y < 0 || y >= height)
            return -1;

        return y * width + x;
    }

    private int FindClosestWalkable(Vector2 position)
    {
        int best = -1;
        float bestDistance = float.PositiveInfinity;

        for (int i = 0; i < blocked.Length; i++)
        {
            if (blocked[i])
                continue;

            float sqr = (
                CellCenter(i) - position
            ).sqrMagnitude;

            if (sqr < bestDistance)
            {
                bestDistance = sqr;
                best = i;
            }
        }

        return best;
    }


    public bool TryGetNextWaypoint(
        Vector2 currentPosition,
        out Vector2 waypoint)
    {
        waypoint = currentPosition;

        if (!ready)
            return false;

        int index = WorldToIndex(currentPosition);

        if (index == -1)
            return false;

        // Si la celda actual no es utilizable,
        // buscar una celda cercana que tenga ruta.
        if (blocked[index] ||
            float.IsPositiveInfinity(distance[index]))
        {
            int closest = -1;
            float bestDistance = float.PositiveInfinity;

            for (int i = 0; i < blocked.Length; i++)
            {
                if (blocked[i] ||
                    float.IsPositiveInfinity(distance[i]))
                    continue;

                Vector2 center = CellCenter(i);

                float sqrDistance =
                    (center - currentPosition).sqrMagnitude;

                // Buscar solamente en un radio cercano.
                if (sqrDistance > cellSize * cellSize * 9f)
                    continue;

                // Evitar elegir una celda detrás de una pared.
                RaycastHit2D wall = Physics2D.Linecast(
                    currentPosition,
                    center,
                    obstacleLayer
                );

                if (wall.collider != null)
                    continue;

                if (sqrDistance < bestDistance)
                {
                    bestDistance = sqrDistance;
                    closest = i;
                }
            }

            if (closest == -1)
                return false;

            waypoint = CellCenter(closest);
            return true;
        }

        int next = nextCell[index];

        if (next == -1)
        {
            waypoint = safeZone.bounds.center;
            return true;
        }

        waypoint = CellCenter(next);
        return true;
    }


    private void OnDrawGizmosSelected()
    {
        if (!showGrid)
            return;

        if (!Application.isPlaying || blocked == null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(
                transform.position +
                new Vector3(
                    width * cellSize * 0.5f,
                    height * cellSize * 0.5f,
                    0f
                ),
                new Vector3(
                    width * cellSize,
                    height * cellSize,
                    0f
                )
            );
            return;
        }

        for (int i = 0; i < blocked.Length; i++)
        {
            Gizmos.color = blocked[i]
                ? new Color(1f, 0f, 0f, 0.3f)
                : float.IsPositiveInfinity(distance[i])
                    ? new Color(1f, 0.6f, 0f, 0.3f)
                    : new Color(0f, 1f, 0f, 0.12f);

            Gizmos.DrawCube(
                CellCenter(i),
                Vector3.one * cellSize * 0.85f
            );
        }
    }

    public string ExplainRouteFailure(Vector2 position)
    {
        if (!ready || blocked == null ||
            distance == null || nextCell == null)
        {
            return "El mapa Dijkstra no está preparado.";
        }

        int index = WorldToIndex(position);

        if (index == -1)
        {
            return "El civil está FUERA de la cuadrícula.";
        }

        if (blocked[index])
        {
            return "La celda del civil está BLOQUEADA.";
        }

        if (float.IsPositiveInfinity(distance[index]))
        {
            return "La celda está libre, pero NO EXISTE " +
                   "un camino conectado hasta ZonaSegura.";
        }

        if (safeZone == null)
        {
            return "No se asignó ZonaSegura.";
        }

        return "La ruta existe. Revisar " +
               "CivilDijkstraNavigator y el estado del civil.";
    }

}
