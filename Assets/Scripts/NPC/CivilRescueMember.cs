
using UnityEngine;

[RequireComponent(typeof(Civilian))]
public class CivilRescueMember : MonoBehaviour
{
    public bool IsRescued { get; private set; }

    private bool isLost;

    private void Start()
    {
        if (CivilRescueManager.Instance != null)
        {
            CivilRescueManager.Instance.RegisterCivil(this);
        }
    }

    public bool Rescue()
    {
        if (IsRescued || isLost)
            return false;

        Civilian civilian = GetComponent<Civilian>();

        if (civilian == null || civilian.isUnconscious)
            return false;

        if (civilian.currentState == AIBase.EnemyState.Die)
            return false;

        IsRescued = true;

        if (CivilRescueManager.Instance != null)
            CivilRescueManager.Instance.RescueCivil(this);

        // Desactivar el civil: ya está fuera del peligro.
        gameObject.SetActive(false);

        return true;
    }

    private void OnDestroy()
    {
        // Si el civil fue destruido por morir,
        // registrarlo como perdido.
        if (!Application.isPlaying || IsRescued || isLost)
            return;

        isLost = true;

        if (CivilRescueManager.Instance != null)
            CivilRescueManager.Instance.CivilLost(this);
    }
}
