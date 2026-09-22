using UnityEngine;

public class EnemyTest : EnemyBase
{
    protected override void Start()
    {
        base.Start();
        currentState = EnemyState.Patrol;
    }
}