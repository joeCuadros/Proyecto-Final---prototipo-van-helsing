
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObstacleEvent : MonoBehaviour
{
    public float investigationDistance = 10f;

    private Collider2D triggerArea;

    private void Awake()
    {
        triggerArea = GetComponent<Collider2D>();
        triggerArea.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ejecuta");
        if (LevelManager.Instance == null) return;
        LevelManager.Instance.InvestigateEnemy(transform.position, investigationDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, investigationDistance);
    }
}
