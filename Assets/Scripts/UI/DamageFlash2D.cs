
using System.Collections;
using UnityEngine;

public class DamageFlash2D : MonoBehaviour
{
    public EntityBase entity;
    public SpriteRenderer spriteRenderer;

    public Color flashColor =
        new Color(1f, 0.25f, 0.25f, 1f);

    public float flashDuration = 0.15f;

    private Color originalColor;
    private Coroutine flashRoutine;

    private void Awake()
    {
        if (entity == null)
            entity = GetComponentInParent<EntityBase>();

        if (spriteRenderer == null)
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        if (entity != null)
            entity.Damaged += OnDamaged;
    }

    private void OnDisable()
    {
        if (entity != null)
            entity.Damaged -= OnDamaged;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    private void OnDamaged(int damage)
    {
        if (spriteRenderer == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        originalColor = spriteRenderer.color;
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(flashDuration);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        flashRoutine = null;
    }
}
