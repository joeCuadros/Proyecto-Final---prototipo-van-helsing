
using UnityEngine;

public class PlayerCarryCivilian : MonoBehaviour
{
    [Header("Punto de carga")]
    [SerializeField] private Transform carryPoint;
    [SerializeField]
    private Vector3 carryOffset =
        new Vector3(0f, 0.5f, 0f);

    [Header("Movimiento")]
    [SerializeField, Range(1f, 2f)]
    private float sprintMultiplier = 1.2f;

    public float SprintMultiplier => sprintMultiplier;

    public CivilRescueMember CarriedMember { get; private set; }

    public bool IsCarrying => CarriedMember != null;

    private Transform originalParent;
    private Rigidbody2D carriedBody;
    private Collider2D[] carriedColliders;
    private bool[] originalColliderStates;
    private bool originalSimulated;
    private bool originalCivilianEnabled;
    private Civilian carriedCivilian;

    public bool TryPickup(Civilian civilian)
    {
        if (IsCarrying || civilian == null)
            return false;

        if (!civilian.isUnconscious ||
            civilian.currentState == AIBase.EnemyState.Die)
            return false;

        CivilRescueMember member =
            civilian.GetComponent<CivilRescueMember>();

        if (member == null || member.IsRescued)
            return false;

        Rigidbody2D body =
            civilian.GetComponent<Rigidbody2D>();

        if (body == null)
            return false;

        CarriedMember = member;
        carriedCivilian = civilian;
        carriedBody = body;

        originalParent = civilian.transform.parent;
        originalSimulated = body.simulated;
        originalCivilianEnabled = civilian.enabled;

        carriedColliders =
            civilian.GetComponentsInChildren<Collider2D>();

        originalColliderStates =
            new bool[carriedColliders.Length];

        for (int i = 0; i < carriedColliders.Length; i++)
        {
            originalColliderStates[i] =
                carriedColliders[i].enabled;

            carriedColliders[i].enabled = false;
        }

        body.linearVelocity = Vector2.zero;
        body.simulated = false;

        // Pausar la IA mientras es transportado.
        civilian.enabled = false;

        Transform anchor = carryPoint != null
            ? carryPoint
            : transform;

        civilian.transform.SetParent(anchor);

        civilian.transform.localPosition =
            carryPoint != null
                ? Vector3.zero
                : carryOffset;

        civilian.transform.localRotation =
            Quaternion.identity;

        Debug.Log("Van Helsing está cargando a " +
                  civilian.name);

        return true;
    }

    public bool TryDeliver()
    {
        if (!IsCarrying)
            return false;

        CivilRescueMember member = CarriedMember;

        // El rescate debe validar al civil cargado.
        bool delivered = member.Rescue(true);

        if (!delivered)
            return false;

        member.transform.SetParent(originalParent, true);

        ClearReferences();

        Debug.Log("Civil cargado entregado en ZonaSegura.");
        return true;
    }

    private void ClearReferences()
    {
        CarriedMember = null;
        carriedCivilian = null;
        carriedBody = null;
        carriedColliders = null;
        originalColliderStates = null;
        originalParent = null;
    }

    public void DropCivilian()
    {
        if (!IsCarrying)
            return;

        carriedCivilian.transform.SetParent(
            originalParent, true
        );

        carriedBody.simulated = originalSimulated;
        carriedBody.linearVelocity = Vector2.zero;

        for (int i = 0; i < carriedColliders.Length; i++)
        {
            if (carriedColliders[i] != null)
            {
                carriedColliders[i].enabled =
                    originalColliderStates[i];
            }
        }

        carriedCivilian.enabled = originalCivilianEnabled;

        ClearReferences();
    }
}
