using UnityEngine;

public class EnemyTest : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.GetComponent<EntityBase>();
        }
        currentState = EnemyState.Wander;
    }
}