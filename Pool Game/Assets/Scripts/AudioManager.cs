using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Slider soundVolumeSlider;
    public AudioSource soundAudioSource, volumeAudioSource;

    public AudioClip clickButtonSFX, StickHit, ballCollide, edgeCollide, pocketedBall, startPoolRack, nextPlayerSelect, crowdCheering;

    OldGameManager gameManagerScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        soundAudioSource = GetComponent<AudioSource>();
        gameManagerScript = GetComponent<OldGameManager>();
        LoadVolumeSettingsPanel();
    }

    public void PlaySound(AudioClip clip)
    {
        soundAudioSource.PlayOneShot(clip);
    }

    public void PlaySoundMechanics(AudioClip clip)
    {
        if(gameManagerScript.MechanicSoundAllow)
        {
            volumeAudioSource.volume = 1;
            volumeAudioSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundMechanicsVolume(AudioClip clip, float currentVolume)
    {
        if(gameManagerScript.MechanicSoundAllow)
        {
            volumeAudioSource.volume = currentVolume;
            volumeAudioSource.PlayOneShot(clip);
        }
    }

    public IEnumerator PlaySoundDuration(AudioClip clip, float delay, int numOfTotalTimes)
    {
        int numOfTimes = 0;
        while(numOfTimes < numOfTotalTimes)
        {
            soundAudioSource.PlayOneShot(clip);
            yield return new WaitForSeconds(delay);
            numOfTimes++;
        }
    }

    public void ChangeSoundVolume()
    {
        soundAudioSource.volume = soundVolumeSlider.value;
        PlayerPrefs.SetFloat("soundVolume", soundVolumeSlider.value);
    }

    void LoadVolumeSettingsPanel()
    {
        soundAudioSource.volume = PlayerPrefs.GetFloat("soundVolume", 1f);
        soundVolumeSlider.value = soundAudioSource.volume;
    }

    public void PlayClickSound() { PlaySound(clickButtonSFX); }
}
