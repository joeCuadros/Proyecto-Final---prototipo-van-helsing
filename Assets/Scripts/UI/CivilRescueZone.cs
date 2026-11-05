
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CivilRescueZone : MonoBehaviour
{
    [Header("Configuracion")]
    public bool showDebugMessages = true;

    private void Reset()
    {
        Collider2D zoneCollider =
            GetComponent<Collider2D>();

        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ProcessEntry(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // También permite entregar si ya estaba
        // dentro de la zona al recoger al civil.
        ProcessEntry(other);
    }

    private void ProcessEntry(Collider2D other)
    {
        // Caso 1: civil que llega caminando.
        CivilRescueMember member =
            other.GetComponentInParent<CivilRescueMember>();

        if (member != null)
        {
            if (member.Rescue() && showDebugMessages)
                Debug.Log("Civil llegó a ZonaSegura.");

            return;
        }

        // Caso 2: Van Helsing llega cargando.
        PlayerCarryCivilian carry =
            other.GetComponentInParent<PlayerCarryCivilian>();

        if (carry != null && carry.IsCarrying)
        {
            if (carry.TryDeliver() && showDebugMessages)
                Debug.Log("Civil cargado rescatado.");
        }
    }
}
