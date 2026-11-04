
using UnityEngine;
using UnityEngine.UI;

public class WorldHealthHearts : MonoBehaviour
{
    [Header("Personaje")]
    public EntityBase entity;

    [Header("Corazones")]
    public Image[] heartImages; 
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Visibilidad")]
    public bool alwaysVisible = false;
    public float visibleAfterDamageSeconds = 2f;

    [Header("Canvas")]
    public Canvas worldCanvas;

    private float hideAt;

    private void Awake()
    {
        if (entity == null)
            entity = GetComponentInParent<EntityBase>();

        if (worldCanvas == null)
            worldCanvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        if (entity != null)
        {
            entity.HealthChanged += Refresh;
            entity.Damaged += ShowAfterDamage;

            Refresh(
                entity.CurrentHealth,
                entity.maxHealth
            );
        }

        SetVisible(alwaysVisible);
    }

    private void OnDisable()
    {
        if (entity != null)
        {
            entity.HealthChanged -= Refresh;
            entity.Damaged -= ShowAfterDamage;
        }
    }

    private void Update()
    {
        if (!alwaysVisible && Time.time >= hideAt)
        {
            SetVisible(false);
        }
    }

    private void ShowAfterDamage(int damage)
    {
        hideAt = Time.time + visibleAfterDamageSeconds;
        SetVisible(true);
    }

    private void Refresh(int health, int maximum)
    {
        if (heartImages == null)
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            Image heart = heartImages[i];

            if (heart == null)
                continue;

            heart.enabled = i < maximum;

            heart.sprite = i < health
                ? fullHeart
                : emptyHeart;
        }
    }

    private void SetVisible(bool visible)
    {
        if (worldCanvas != null)
            worldCanvas.enabled = visible;
    }
}
