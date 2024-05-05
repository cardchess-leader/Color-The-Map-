using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hyperbyte;

public class AudioController : Singleton<AudioController>
{
    [Header("Audio Soureces")]
    public AudioSource audioSource;
    public AudioSource lowSoundSource;

    float lowAudioDefaultVolume = 0.1F;

    [Header("Audio Clips")]
    public AudioClip btnPressSound;

    public void PlayClip(AudioClip clip)
    {
        if (ProfileManager.Instance.IsSoundEnabled)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void PlayClipLow(AudioClip clip)
    {
        if (ProfileManager.Instance.IsSoundEnabled)
        {
            lowSoundSource.volume = lowAudioDefaultVolume;
            lowSoundSource.PlayOneShot(clip);
        }
    }

    public void PlayClipLow(AudioClip clip, float volume)
    {
        if (ProfileManager.Instance.IsSoundEnabled)
        {
            lowSoundSource.volume = volume;
            lowSoundSource.PlayOneShot(clip);
        }
    }

    public void PlayButtonClickSound()
    {
        if (ProfileManager.Instance.IsSoundEnabled)
        {
            audioSource.PlayOneShot(btnPressSound);
        }
    }
}