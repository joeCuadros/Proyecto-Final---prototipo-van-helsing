using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource loopSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SetupAudioSources()
    {
        // 1. Creamos el de la Musica (BGM) desde cero
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        // 2. Creamos el de los Efectos (SFX) desde cero
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        // 3. Creamos el de los Efectos en loop (correr, etc.)
        loopSource = gameObject.AddComponent<AudioSource>();
        loopSource.loop = true;
        loopSource.playOnAwake = false;
    }
    // Funciones para el fondo de musica
    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmClip == null) return;
        if (bgmSource.clip == bgmClip) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
    // Funciones para efectos de sonidos
    public void PlaySFX(AudioClip sfxClip)
    {
        if (sfxClip == null) return;
        sfxSource.PlayOneShot(sfxClip);
    }
    // Funciones para efectos de sonido en loop
    public void PlayLoopSFX(AudioClip loopClip)
    {
        if (loopClip == null) return;
        if (loopSource.clip == loopClip && loopSource.isPlaying) return;

        loopSource.clip = loopClip;
        loopSource.Play();
    }

    public void StopLoopSFX()
    {
        loopSource.Stop();
    }
}