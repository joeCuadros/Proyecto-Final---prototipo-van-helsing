using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public class EnemyCountData
    {
        public int cantidad_total = 0;
        public int cantidad_viva = 0;
    }
    private readonly object _registryLock = new object();
    private Dictionary<string, EnemyCountData> enemyRegistry = new Dictionary<string, EnemyCountData>();
    private List<AIBase> enemies = new List<AIBase>();
    private int totalEnemiesAlive = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterEnemy(string enemyId, AIBase enemy)
    {
        lock (_registryLock) // candado mutex
        {
            enemies.Add(enemy);
            if (!enemyRegistry.ContainsKey(enemyId))
            {
                enemyRegistry[enemyId] = new EnemyCountData(); // si no hay nada lo creamos
            }
            enemyRegistry[enemyId].cantidad_total++;
            enemyRegistry[enemyId].cantidad_viva++;
            totalEnemiesAlive++;
            if (UIManager.Instance != null) UIManager.Instance.UpdateEnemyCounters(enemyRegistry, totalEnemiesAlive);
        }
    }

    public void EnemyDied(string enemyId, AIBase enemy)
    {
        lock (_registryLock) // candado mutex
        {
            if (!enemies.Remove(enemy)) return;
            if (enemyRegistry.ContainsKey(enemyId) && enemyRegistry[enemyId].cantidad_viva > 0)
            {
                enemyRegistry[enemyId].cantidad_viva--;
                totalEnemiesAlive--;
                if (UIManager.Instance != null) UIManager.Instance.UpdateEnemyCounters(enemyRegistry, totalEnemiesAlive);
                if (totalEnemiesAlive <= 0)
                {
                    TriggerVictory();
                }
            }
        }
    }

    private void TriggerVictory()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.Victory();
    }

    public AIBase GetEnemy(int index)
    {
        lock (_registryLock)
        {
            if (index < 0 || index >= enemies.Count) return null;
            return enemies[index];
        }
    }

    public void InvestigateEnemy(Vector2 point, float distance)
    {
        lock (_registryLock)
        {
            foreach (AIBase enemy in enemies)
            {
                if (enemy == null) continue;

                if (Vector2.Distance(enemy.transform.position, point) <= distance)
                {
                    enemy.investigatePoint = point;
                    enemy.currentState = AIBase.EnemyState.Investigate;
                }
            }
        }
    }
}