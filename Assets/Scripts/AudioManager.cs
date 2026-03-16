using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public bool isMusicOn { get; private set; }
    public bool isSoundOn { get; private set; }
    public bool isShakeOn { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM")]
    [SerializeField] private AudioClip bgmClip;
    // [Range(0f, 1f)] public float bgmVolume = 0.5f;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip tapSound;
    [SerializeField] private AudioClip removeSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlayBGM();
    }

    private void LoadSettings()
    {
        isMusicOn = PlayerPrefs.GetInt("MusicOn", 1) == 1;
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
        isShakeOn = PlayerPrefs.GetInt("ShakeOn", 1) == 1;
        UpdateAudioState();
    }

    private void UpdateAudioState()
    {
        if (bgmSource != null) bgmSource.mute = !isMusicOn;
        if (sfxSource != null) sfxSource.mute = !isSoundOn;
    }

    public void PlayTapSound()
    {
        if (sfxSource != null && tapSound != null && isSoundOn)
        {
            sfxSource.PlayOneShot(tapSound);
        }
    }

    public void PlayRemoveSound()
    {
        if (sfxSource != null && removeSound != null)
        {
            sfxSource.PlayOneShot(removeSound);
        }
    }

    public void PlayWinSound()
    {
        if (sfxSource != null && winSound != null)
        {
            sfxSource.PlayOneShot(winSound);
        }
    }

    public void PlayLoseSound()
    {
        if (sfxSource != null && loseSound != null)
        {
            sfxSource.PlayOneShot(loseSound);
        }
    }

    // --- Background Music ---
    public void PlayBGM()
    {
        if (bgmSource == null || bgmClip == null) return;
        bgmSource.clip = bgmClip;
        // bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null) bgmSource.Stop();
    }

    public void PauseBGM()
    {
        if (bgmSource != null) bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (bgmSource != null) bgmSource.UnPause();
    }

    // --- Settings Toggles ---
    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateAudioState();
    }

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
        UpdateAudioState();
    }

    public void ToggleShake()
    {
        isShakeOn = !isShakeOn;
        PlayerPrefs.SetInt("ShakeOn", isShakeOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void Vibrate()
    {
        if (isShakeOn)
        {
            Handheld.Vibrate();
        }
    }
}
