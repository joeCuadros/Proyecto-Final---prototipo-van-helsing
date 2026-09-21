using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerCombat combat;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
    }

    void OnEnable()
    {
        if (movement != null)
        {
            movement.OnDirectionChanged += UpdateMoveAnimation;
            movement.OnSprintChanged += UpdateSprintAnimation;
        }
        if (combat != null)
        {
            combat.OnAttackFired += TriggerAttackAnimation;
        }
    }

    void OnDisable()
    {
        if (movement != null)
        {
            movement.OnDirectionChanged -= UpdateMoveAnimation;
            movement.OnSprintChanged -= UpdateSprintAnimation;
        }
        if (combat != null)
        {
            combat.OnAttackFired -= TriggerAttackAnimation;
        }
    }

    private void UpdateMoveAnimation(Vector2 dir)
    {
        //Debug.Log($"Direccion x={dir.x}, y={dir.y}");
    }

    private void UpdateSprintAnimation(bool isSprinting)
    {
        //Debug.Log($"Sprint es {isSprinting}");
    }
    private void TriggerAttackAnimation(WeaponType weaponUsed)
    {
        Debug.Log($"Atacando con {weaponUsed.ToString()}");
    }
}