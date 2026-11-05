using UnityEngine;

public class TestSceneAudio : MonoBehaviour
{
    public AudioClip backgroundMusic;
    public AudioClip ambientHowl;

    public float firstHowlDelay = 15f;
    public float howlInterval = 30f;

    private void Start()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(backgroundMusic);

        if (ambientHowl != null)
        {
            InvokeRepeating(
                nameof(PlayHowl),
                Mathf.Max(1f, firstHowlDelay),
                Mathf.Max(5f, howlInterval)
            );
        }
    }

    private void PlayHowl()
    {
        if (Time.timeScale <= 0f)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(ambientHowl);
    }

    private void OnDisable()
    {
        CancelInvoke();
    }
}