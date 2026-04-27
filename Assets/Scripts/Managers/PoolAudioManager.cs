using UnityEngine;
using UnityEngine.Audio;

public class PoolAudioManager : MonoBehaviour
{
    public static PoolAudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource sfx2DSource;
    [SerializeField] private AudioSource sfx3DPrefab;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";
    [SerializeField] private string uiVolumeParam = "UIVolume";

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool playMusicOnStart = true;

    [Header("UI")]
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip hoverClip;

    [Header("Cue / Ball Impacts")]
    [SerializeField] private AudioClip[] cueHitClips;
    [SerializeField] private AudioClip[] ballCollisionClips;
    [SerializeField] private AudioClip[] railHitClips;

    [Header("Pocket Sounds")]
    [SerializeField] private AudioClip cueBallPocketedRightClip;
    [SerializeField] private AudioClip cueBallPocketedWrongClip;

    [SerializeField] private AudioClip solidPocketedRightClip;
    [SerializeField] private AudioClip solidPocketedWrongClip;

    [SerializeField] private AudioClip stripePocketedRightClip;
    [SerializeField] private AudioClip stripePocketedWrongClip;

    [SerializeField] private AudioClip currencyPocketedClip;

    [Header("Game State")]
    [SerializeField] private AudioClip gameWinClip;
    [SerializeField] private AudioClip gameLoseClip;
    [SerializeField] private AudioClip gameEndClip;

    [Header("Pitch Variation")]
    [SerializeField] private bool usePitchVariation = true;
    [SerializeField] private Vector2 sfxPitchRange = new Vector2(0.95f, 1.05f);

    [Header("2D Volumes")]
    [Range(0f, 1f)][SerializeField] private float musicVolume = 0.8f;
    [Range(0f, 1f)][SerializeField] private float uiVolume = 1f;
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureSources();
        ApplyLocalVolumes();
    }

    private void Start()
    {
        if (playMusicOnStart && backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void EnsureSources()
    {
        if (musicSource == null)
        {
            GameObject obj = new GameObject("MusicSource");
            obj.transform.SetParent(transform);
            musicSource = obj.AddComponent<AudioSource>();
        }

        if (uiSource == null)
        {
            GameObject obj = new GameObject("UISource");
            obj.transform.SetParent(transform);
            uiSource = obj.AddComponent<AudioSource>();
        }

        if (sfx2DSource == null)
        {
            GameObject obj = new GameObject("SFX2DSource");
            obj.transform.SetParent(transform);
            sfx2DSource = obj.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;

        uiSource.loop = false;
        uiSource.playOnAwake = false;
        uiSource.spatialBlend = 0f;

        sfx2DSource.loop = false;
        sfx2DSource.playOnAwake = false;
        sfx2DSource.spatialBlend = 0f;
    }

    private void ApplyLocalVolumes()
    {
        musicSource.volume = musicVolume;
        uiSource.volume = uiVolume;
        sfx2DSource.volume = sfxVolume;
    }

    private float GetRandomPitch()
    {
        if (!usePitchVariation)
            return 1f;

        return Random.Range(sfxPitchRange.x, sfxPitchRange.y);
    }

    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.pitch = 1f;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void PlayButtonClick()
    {
        PlayUI(buttonClickClip);
    }

    public void PlayHover()
    {
        PlayUI(hoverClip);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip == null) return;

        uiSource.pitch = 1f;
        uiSource.PlayOneShot(clip, uiVolume);
    }

    public void PlaySFX2D(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip == null) return;

        sfx2DSource.pitch = GetRandomPitch();
        sfx2DSource.PlayOneShot(clip, sfxVolume * volumeMultiplier);
    }

    public void PlaySFX3D(AudioClip clip, Vector3 worldPosition, float volumeMultiplier = 1f)
    {
        if (clip == null || sfx3DPrefab == null) return;

        AudioSource temp = Instantiate(sfx3DPrefab, worldPosition, Quaternion.identity);
        temp.pitch = GetRandomPitch();
        temp.volume = sfxVolume * volumeMultiplier;
        temp.clip = clip;
        temp.Play();

        Destroy(temp.gameObject, clip.length + 0.2f);
    }

    public void PlayCueHit(Vector3? worldPosition = null, float strength = 1f)
    {
        AudioClip clip = GetRandomClip(cueHitClips);
        if (worldPosition.HasValue)
            PlaySFX3D(clip, worldPosition.Value, Mathf.Clamp01(strength));
        else
            PlaySFX2D(clip, Mathf.Clamp01(strength));
    }

    public void PlayBallCollision(Vector3? worldPosition = null, float strength = 1f)
    {
        AudioClip clip = GetRandomClip(ballCollisionClips);
        float volume = Mathf.Clamp(Mathf.Lerp(0.2f, 1f, strength), 0f, 1f);

        if (worldPosition.HasValue)
            PlaySFX3D(clip, worldPosition.Value, volume);
        else
            PlaySFX2D(clip, volume);
    }

    public void PlayRailHit(Vector3? worldPosition = null, float strength = 1f)
    {
        AudioClip clip = GetRandomClip(railHitClips);
        float volume = Mathf.Clamp(Mathf.Lerp(0.2f, 1f, strength), 0f, 1f);

        if (worldPosition.HasValue)
            PlaySFX3D(clip, worldPosition.Value, volume);
        else
            PlaySFX2D(clip, volume);
    }

    public void PlayCueBallPocketed(bool correct)
    {
        PlaySFX2D(correct ? cueBallPocketedRightClip : cueBallPocketedWrongClip);
    }

    public void PlaySolidPocketed(bool correct)
    {
        PlaySFX2D(correct ? solidPocketedRightClip : solidPocketedWrongClip);
    }

    public void PlayStripePocketed(bool correct)
    {
        PlaySFX2D(correct ? stripePocketedRightClip : stripePocketedWrongClip);
    }

    public void PlayCurrencyPocketed()
    {
        PlaySFX2D(currencyPocketedClip);
    }

    public void PlayGameWin()
    {
        PlaySFX2D(gameWinClip);
    }

    public void PlayGameLose()
    {
        PlaySFX2D(gameLoseClip);
    }

    public void PlayGameEnd()
    {
        PlaySFX2D(gameEndClip);
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicSource.volume = musicVolume;
        SetMixerVolume(musicVolumeParam, musicVolume);
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        sfx2DSource.volume = sfxVolume;
        SetMixerVolume(sfxVolumeParam, sfxVolume);
    }

    public void SetUIVolume(float value)
    {
        uiVolume = Mathf.Clamp01(value);
        uiSource.volume = uiVolume;
        SetMixerVolume(uiVolumeParam, uiVolume);
    }

    private void SetMixerVolume(string parameterName, float normalizedValue)
    {
        if (audioMixer == null || string.IsNullOrWhiteSpace(parameterName))
            return;

        float dB = normalizedValue <= 0.0001f ? -80f : Mathf.Log10(normalizedValue) * 20f;
        audioMixer.SetFloat(parameterName, dB);
    }
}