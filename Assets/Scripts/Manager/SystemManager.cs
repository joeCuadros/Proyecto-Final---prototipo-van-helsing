using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SystemManager : MonoBehaviour
{
    public static SystemManager Instance { get; private set; }
    private bool isPaused = false;
    private bool isGameEnded = true;

    [Header("Audio")]
    public AudioClip gameOverClip;
    private PlayerInput playerInput;
    private Dictionary<int, string> sceneRoutes = new Dictionary<int, string>()
    {
        { 0, "MainMenu" },
        { 1, "Nivel 1" }, //Level_01
        { 2, "Nivel 2" }, //Level_02
    };
    public int currentSceneKey { get; private set; } = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(int sceneKey)
    {
        if (sceneRoutes.TryGetValue(sceneKey, out string exactSceneName))
        {
            Time.timeScale = 1f;
            isPaused = false; 
            currentSceneKey = sceneKey;
            //hasta que se registre el jugador
            isGameEnded = true;
            if (playerInput != null)
            {
                playerInput.SwitchCurrentActionMap("System");
                playerInput = null;
            }

            SceneManager.LoadScene(exactSceneName);
        }
    }
    public void NextScene()
    {
        int nextKey = currentSceneKey + 1;
        if (sceneRoutes.ContainsKey(nextKey))
        {
            LoadScene(nextKey);
        }
        else
        {
            LoadScene(currentSceneKey);
        }
    }
    public void RestartScene()
    {
        LoadScene(currentSceneKey);
    }
    public void InitScene()
    {
        LoadScene(0);
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
        if (isGameEnded) return;
        isGameEnded = true;
        Time.timeScale = 0f;
        playerInput.SwitchCurrentActionMap("System");
        if (UIManager.Instance != null) UIManager.Instance.RenderGameOver();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopLoopSFX();
            AudioManager.Instance.StopBGM();
            AudioManager.Instance.PlaySFX(gameOverClip);
        }
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