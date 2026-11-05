
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interaccion con civiles")]
    [SerializeField] private float interactionRadius = 1.5f;
    [SerializeField] private LayerMask civilianLayer;
    [SerializeField] private LayerMask obstacleLayer;

    private PlayerCarryCivilian carry;

    private void Awake()
    {
        carry = GetComponent<PlayerCarryCivilian>();
    }

    private void OnInteract(InputValue value)
    {
        if (!value.isPressed)
            return;

        TryInteractWithCivilian();
    }

    private void TryInteractWithCivilian()
    {
        if (carry != null && carry.IsCarrying)
        {
            Debug.Log("Van Helsing ya está cargando un civil.");
            return;
        }

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

            if (civilian == null ||
                civilian.currentState == AIBase.EnemyState.Die)
                continue;

            CivilRescueMember member =
                civilian.GetComponent<CivilRescueMember>();

            if (member != null && member.IsRescued)
                continue;

            float distance = Vector2.Distance(
                transform.position,
                civilian.transform.position
            );

            if (distance >= closestDistance)
                continue;

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

        if (closestCivilian == null)
        {
            Debug.Log("No hay civiles al alcance.");
            return;
        }

        // Civil desmayado: recoger.
        if (closestCivilian.isUnconscious)
        {
            if (carry != null &&
                carry.TryPickup(closestCivilian))
            {
                Debug.Log("Civil inconsciente recogido.");
            }

            return;
        }

        // Civil consciente: ayuda normal.
        closestCivilian.HelpByVanHelsing();

        Debug.Log(
            "Van Helsing ayudó a " +
            closestCivilian.name
        );
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
