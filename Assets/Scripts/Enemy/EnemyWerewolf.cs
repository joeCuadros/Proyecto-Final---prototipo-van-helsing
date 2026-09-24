using UnityEngine;

public class EnemyWerewolf : EnemyBase
{
    protected override void Start()
    {
        enemyType = "Werewolf";
        base.Start();
    }
}