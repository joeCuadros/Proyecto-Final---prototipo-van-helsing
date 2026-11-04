
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaccion con civiles")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask civilianLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        TryHelpCivilian();
    }

    private void TryHelpCivilian()
    {
        Collider2D[] detected = Physics2D.OverlapCircleAll(
            transform.position,
            interactionRadius,
            civilianLayer
        );

        Civilian closestCivilian = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in detected)
        {
            Civilian civilian =
                hit.GetComponentInParent<Civilian>();

            if (civilian == null)
                continue;

            if (civilian.isUnconscious)
                continue;

            if (civilian.currentState == AIBase.EnemyState.Die)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                civilian.transform.position
            );

            if (distance >= closestDistance)
                continue;

            // No permitir ayudar a través de paredes.
            RaycastHit2D wall = Physics2D.Linecast(
                transform.position,
                civilian.transform.position,
                obstacleLayer
            );

            if (wall.collider != null)
                continue;

            closestCivilian = civilian;
            closestDistance = distance;
        }

        if (closestCivilian != null)
        {
            closestCivilian.HelpByVanHelsing();

            Debug.Log(
                "Van Helsing ayudó a " +
                closestCivilian.name
            );
        }
        else
        {
            Debug.Log(
                "No hay civiles conscientes al alcance."
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            interactionRadius
        );
    }
}
