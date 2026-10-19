using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerCombat combat;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public AudioClip sprintClip;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0); // animacion actual
                                                                           
        if (!(state.IsName("Beside") || state.IsName("Up") || state.IsName("Down"))) 
            return; // Solo cambiar estados de movimiento

        
        // Ariba o abajo
        if (dir.x != 0)
        {
            if (!state.IsName("Beside")) animator.Play("Beside");

            if (spriteRenderer != null)
                spriteRenderer.flipX = dir.x < 0;
            return;
        }


        // Segun direccion
        if (dir.y > 0)
        {
            if (!state.IsName("Down")) animator.Play("Down");
        }
        else
        {
            if (!state.IsName("Up")) animator.Play("Up");
        }
    }

    private void UpdateSprintAnimation(bool isSprinting)
    {
        //Debug.Log($"Sprint es {isSprinting}");
        if (AudioManager.Instance == null && sprintClip != null) return;
        if (isSprinting && sprintClip != null)
        {
            AudioManager.Instance.PlayLoopSFX(sprintClip);
        }
        else
        {
            AudioManager.Instance.StopLoopSFX();
        }
    }
    private void TriggerAttackAnimation(WeaponType weaponUsed)
    {
        switch (weaponUsed)
        {
            case WeaponType.Dagger:
                animator.Play("Daga");
                break;

            case WeaponType.SilverBullet:
                animator.Play("Shoot");
                break;

            case WeaponType.WoodenStake:
                animator.Play("Shoot");
                break;

            default:
                Debug.Log($"Atacando con {weaponUsed}");
                break;
        }
    }
}