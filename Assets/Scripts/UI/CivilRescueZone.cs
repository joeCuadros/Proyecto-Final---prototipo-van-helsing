
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CivilRescueZone : MonoBehaviour
{
    [Header("Configuracion")]
    public bool showDebugMessages = true;

    private void Reset()
    {
        Collider2D zoneCollider = GetComponent<Collider2D>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CivilRescueMember member =
            other.GetComponentInParent<CivilRescueMember>();

        if (member == null)
            return;

        bool rescued = member.Rescue();

        if (rescued && showDebugMessages)
        {
            Debug.Log(
                "El civil llegó a la zona segura."
            );
        }
    }
}
