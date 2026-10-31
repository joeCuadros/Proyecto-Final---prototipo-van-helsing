using UnityEngine;

public class EnemyDemon: AIBase
{
    protected override void Start()
    {
        enemyType = "Demon";
        base.Start();
    }
}
