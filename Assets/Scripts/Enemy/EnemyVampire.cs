using UnityEngine;

public class EnemyVampire : EnemyBase
{
    protected override void Start()
    {
        enemyType = "Vampire";
        base.Start();
    }
}