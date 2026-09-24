using UnityEngine;
using UnityEngine.InputSystem;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }
    private bool isPaused = true;
    private bool isGameEnded = true;

    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerInput nuevoPlayerInput)
    {
        playerInput = nuevoPlayerInput;
        isGameEnded = false;
    }

    public void TogglePause()
    {
        if (isGameEnded || playerInput == null) return; // si esta termino no hacer nada

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
        playerInput.SwitchCurrentActionMap(isPaused ? "System" : "Player");
        if (UIManager.Instance != null) UIManager.Instance.RenderPause(isPaused);
    }

    public void GameOver()
    {
        if (playerInput == null) return;
        isGameEnded = true;
        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("System");
        if (UIManager.Instance != null) UIManager.Instance.RenderGameOver();
    }

    public void Victory()
    {
        if (playerInput == null) return;
        isGameEnded = true;
        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("System");
        if (UIManager.Instance != null)  UIManager.Instance.RenderVictory();
    }

    // eventos externos 
    void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            TogglePause();
        }
    }
}