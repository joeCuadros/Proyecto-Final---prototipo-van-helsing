using UnityEngine;
using UnityEngine.InputSystem;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }
    public bool isPaused { get; private set; }
    public bool isGameOver { get; private set; }

    private PlayerInput playerInput;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterPlayer(PlayerInput nuevoPlayerInput)
    {
        playerInput = nuevoPlayerInput;
        Debug.Log("Sistema: Jugador registrado correctamente.");
    }

    public void TogglePause()
    {
        if (isGameOver || playerInput == null) return; // si esta muerto no hacer nada

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
        {
            playerInput.SwitchCurrentActionMap("System");
        }
        else
        {
            playerInput.SwitchCurrentActionMap("Player");
        }
        Debug.Log($"Sistema: Pausa = {isPaused}. Control actual = {playerInput.currentActionMap.name}"); // mandar a UI
    }

    void OnPause(InputValue value)
    {
        if (value.isPressed)
        {
            TogglePause();
        }
    }

    public void GameOver()
    {
        if (playerInput == null) return;
        isGameOver = true;
        playerInput.SwitchCurrentActionMap("System");   
        Debug.Log("Sistema: ¡Game Over! Controles de jugador desactivados.");
    }
}