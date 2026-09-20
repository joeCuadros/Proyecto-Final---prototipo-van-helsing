using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    private bool canAttack = true; // interruptor maestro

    public void SetCombatEnabled(bool state)
    {
        canAttack = state;
    }

    void OnAttack()
    {
        if (!canAttack) return;
        Debug.Log("Combat: ¡Atacando!");
    }
}