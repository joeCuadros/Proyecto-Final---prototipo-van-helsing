using UnityEngine;

public class EnemyWerewolf : AIBase
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    public override void TakeDamage(int amount, WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Dagger:
            case WeaponType.SilverBullet:
            case WeaponType.WoodenStake:
                base.TakeDamage(amount);
                break;
            case WeaponType.HolyWater: // invulnerable
                break;

            default:
                base.TakeDamage(amount);
                break;
        }
    }

    protected override void Start()
    {
        enemyType = "Werewolf";
        base.Start();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        UpdateMoveAnimation(velocity);
    }

    public override void AttackTarget(EntityBase target)
    {
        base.AttackTarget(target);
        Vector2 direction = target.transform.position - transform.position;
       animator.Play("AttackDer"); //Attack Izquierda 
   
    }

    private void UpdateMoveAnimation(Vector2 dir)
    {
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0); // animacion actual

        if (!(state.IsName("Beside") || state.IsName("Up") || state.IsName("Down")))
            return; // Solo cambiar estados de movimiento

        // Izquierda o derecha
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
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
}