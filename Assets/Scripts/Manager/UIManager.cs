using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public enum ModalType { Pause, GameOver, Victory }

    [Header("Configuración de Vida UI")]
    public Transform heartsContainer;     // Tu contenedor vacío
    public Sprite heartFullSprite;
    public Sprite heartBrokenSprite;
    public Sprite shieldSprite;
    public Image staminaBarFill;

    [Header("Configuración de Armas UI")]
    public Color colorSeleccionado = new Color(0.8f, 0.8f, 0.2f, 1f);
    public Color colorNormal = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public Transform armor1; 
    public Transform armor2;
    public Transform armor3; 
    public Transform armor4;

    [Header("Pantalla Universal (Pausa/Victoria/GameOver)")]
    public GameObject modalPanel;
    public GameObject modalHUD;
    public TextMeshProUGUI modalTitle;

    [Header("Botones de Acción")]
    public GameObject resumeButton;         // Boton "Continuar"
    public GameObject restartButton;        // Boton "Reiniciar"
    public GameObject nextLevelButton;      // Boton "Siguiente Nivel"
    public GameObject menuButton;           // Boton "Salir al Menú"

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        if (modalHUD != null) modalHUD.SetActive(true);
        if (modalPanel != null) modalPanel.SetActive(false);
    }

    // Funciones generales del sistema de niveles
    public void RenderGameOver()
    {
        ShowModal(ModalType.GameOver);
    }
    public void RenderVictory()
    {
        ShowModal(ModalType.Victory);
    }
    public void RenderPause(bool isPaused)
    {
        if (isPaused)
        {
            ShowModal(ModalType.Pause);
        }
        else
        {
            if (modalPanel != null) modalPanel.SetActive(false);
            if (modalHUD != null) modalHUD.SetActive(true);
        }
    }
    private void ShowModal(ModalType type)
    {
        if (modalPanel == null) return;
        if (modalHUD != null) modalHUD.SetActive(false);
        modalPanel.SetActive(true);
        resumeButton.SetActive(false);
        restartButton.SetActive(false);
        nextLevelButton.SetActive(false);
        menuButton.SetActive(true);

        switch (type)
        {
            case ModalType.Pause:
                modalTitle.text = "PAUSA";
                modalTitle.color = Color.white;

                resumeButton.SetActive(true);
                restartButton.SetActive(true); 
                break;

            case ModalType.GameOver:
                modalTitle.text = "FIN DEL JUEGO";
                modalTitle.color = Color.red;

                restartButton.SetActive(true); // Solo reiniciar o salir
                break;

            case ModalType.Victory:
                modalTitle.text = "¡NIVEL COMPLETADO!";
                modalTitle.color = Color.yellow;

                nextLevelButton.SetActive(true); // Solo pasar al siguiente o salir
                break;
        }
    }

    // Funciones de stats jugador
    public void ChangeStamina(float currentStamina, float maxStamina)
    {
        if (staminaBarFill != null && maxStamina > 0f)
        {
            staminaBarFill.fillAmount = currentStamina / maxStamina;
        }
    }
    public void UpdateHealthUI(int currentHealth, int maxHealth, bool hasShield)
    {
        if (heartsContainer == null)
            return;

        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        float startX = 0f;     // Pos X inicial
        float startY = 0f;    // Pos Y inicial
        float offsetX = 50f;    // Separación

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject newIcon = new GameObject("Heart_" + i);
            newIcon.transform.SetParent(heartsContainer, false);
            Image imgComponent = newIcon.AddComponent<Image>();
            imgComponent.sprite = (i < currentHealth) ? heartFullSprite : heartBrokenSprite;

            RectTransform rect = newIcon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            rect.localScale = new Vector3(0.5f, 0.5f, 1f);
            rect.anchoredPosition = new Vector2(startX + (offsetX * i), startY);
        }

        if (hasShield)
        {
            GameObject shieldIcon = new GameObject("Shield");
            shieldIcon.transform.SetParent(heartsContainer, false);

            Image imgComponent = shieldIcon.AddComponent<Image>();
            imgComponent.sprite = shieldSprite;

            RectTransform rect = shieldIcon.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100f, 100f);
            rect.localScale = new Vector3(0.5f, 0.5f, 1f);
            rect.anchoredPosition = new Vector2(startX + (offsetX * maxHealth), startY);
        }
    }
    // Armas
    public void RefreshWeaponsUI(WeaponType activeWeapon, bool hasDagger, int stakes, int bullets, int holyWater)
    {
        UpdateSlot(armor1, activeWeapon == WeaponType.Dagger, hasDagger, -1); // 1. DAGA 
        UpdateSlot(armor2, activeWeapon == WeaponType.WoodenStake, stakes > 0, stakes); // 2. ESTACAS
        UpdateSlot(armor3, activeWeapon == WeaponType.SilverBullet, bullets > 0, bullets); // 3. BALAS DE PLATA
        UpdateSlot(armor4, activeWeapon == WeaponType.HolyWater, holyWater > 0, holyWater); // 4. AGUA BENDITA
    }
    private void UpdateSlot(Transform armorParent, bool isSelected, bool isAvailable, int ammoCount)
    {
        if (armorParent == null) return;

        // Extrae los componentes
        Image fondoDestacado = armorParent.Find("Fondo").GetComponent<Image>();
        Image iconoArma = armorParent.Find("image").GetComponent<Image>();
        TextMeshProUGUI textoMunicion = armorParent.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();

        Color colorFondoActual = isSelected ? colorSeleccionado : colorNormal;
        fondoDestacado.color = colorFondoActual;

        Color colorOpuestoTexto = new Color(1f - colorFondoActual.r, 1f - colorFondoActual.g, 1f - colorFondoActual.b, 1f);

        if (isAvailable)
        {
            iconoArma.color = Color.white; // Icono normal
            textoMunicion.color = colorOpuestoTexto; // El texto toma el color contrario al fondo
            textoMunicion.text = (ammoCount == -1) ? "" : ammoCount.ToString();
        }
        else
        {
            iconoArma.color = Color.red; // Solo el icono se pone rojo
            textoMunicion.text = ""; // Quitamos la "X", lo dejamos completamente vacío
        }
    }
    // Enemigos 
    public void UpdateEnemyCounters(Dictionary<string, LevelManager.EnemyCountData> enemyData, int totalAlive)
    {
        Debug.Log($"Total de enemigos vivos en la arena: {totalAlive}");

        foreach (var entry in enemyData)
        {
            string enemyType = entry.Key;
            int total = entry.Value.cantidad_total;
            int vivos = entry.Value.cantidad_viva;

            Debug.Log($"UI -> {enemyType} | Total aparecidos: {total} | Vivos actualmente: {vivos}");
        }
    }
    // Evento de botones
    public void OnClickResume()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.TogglePause();
    }

    public void OnClickRestart()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.RestartScene();
    }

    public void OnClickNextLevel()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.NextScene();
    }

    public void OnClickMainMenu()
    {
        if (SystemManager.Instance != null) SystemManager.Instance.InitScene();
    }
}
