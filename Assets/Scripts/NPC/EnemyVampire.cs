using UnityEngine;

public class EnemyVampire : AIBase
{
    protected override void Start()
    {
        enemyType = "Vampire";
        base.Start();
    }
}