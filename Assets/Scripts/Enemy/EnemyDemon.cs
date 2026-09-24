using UnityEngine;

public class EnemyDemon: EnemyBase
{
    protected override void Start()
    {
        enemyType = "Demon";
        base.Start();
    }
}
