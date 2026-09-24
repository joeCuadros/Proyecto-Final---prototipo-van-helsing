using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Funciones generales del sistema de niveles
    public void RenderGameOver()
    {
        Debug.Log("UI: ¡Game Over!");
    }
    public void RenderVictory()
    {
        Debug.Log("UI: ¡Victory!");
    }
    public void RenderPause(bool isPaused)
    {
        Debug.Log($"UI: Pausa = {isPaused}");
    }
    // Funciones de stats jugador
    public void ChangeStamina(float currentStamina, float maxStamina)
    {
        Debug.Log($"Stats(UI): Vigor actual: {currentStamina}/{maxStamina}");
    }
    public void ChangeHealth(float currentHealth, float maxHealth)
    {
        Debug.Log($"Stats(UI): Vida actual: {currentHealth}/{maxHealth}");
    }
    // Funciones de cambio de armas
    public void RefreshWeaponsUI(WeaponType activeWeapon, bool hasDagger, int stakes, int bullets, int holyWater)
    {

        // 1. DAGA (Infinita)
        string daggerText = hasDagger ? "∞" : "X";
        bool daggerActive = hasDagger;
        // UI Visual: Si !daggerActive -> Poner icono gris o activar imagen de 'X'
        // UI Visual: Si activeWeapon == WeaponType.Dagger -> Resaltar el borde del icono
        Debug.Log($"Slot 1 [Daga]: {(activeWeapon == WeaponType.Dagger ? ">> SELECCIONADA <<" : "")} Ammo: {daggerText}");

        // 2. ESTACAS
        // UI Visual: Si stakes == 0 -> Poner icono gris o activar imagen de 'X'
        // UI Visual: Si activeWeapon == WeaponType.WoodenStake -> Resaltar el borde
        Debug.Log($"Slot 2 [Estaca]: {(activeWeapon == WeaponType.WoodenStake ? ">> SELECCIONADA <<" : "")} Ammo: {stakes}");

        // 3. BALAS DE PLATA
        // UI Visual: Si bullets == 0 -> Poner icono gris o activar imagen de 'X'
        // UI Visual: Si activeWeapon == WeaponType.SilverBullet -> Resaltar el borde
        Debug.Log($"Slot 3 [Bala]: {(activeWeapon == WeaponType.SilverBullet ? ">> SELECCIONADA <<" : "")} Ammo: {bullets}");

        // 4. AGUA BENDITA
        // UI Visual: Si holyWater == 0 -> Poner icono gris o activar imagen de 'X'
        // UI Visual: Si activeWeapon == WeaponType.HolyWater -> Resaltar el borde
        Debug.Log($"Slot 4 [Agua B.]: {(activeWeapon == WeaponType.HolyWater ? ">> SELECCIONADA <<" : "")} Ammo: {holyWater}");
    }
}
