using UnityEngine;
using System;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clip Library")]
    public Sound[] soundLibrary;

    private Dictionary<string, AudioClip> _soundDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _soundDictionary = new Dictionary<string, AudioClip>();
        foreach (Sound sound in soundLibrary)
        {
            _soundDictionary[sound.name] = sound.clip;
        }
    }

    public void PlayMusic(string name)
    {
        if (_soundDictionary.TryGetValue(name, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void PlaySFX(string name)
    {
        if (_soundDictionary.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}