using UnityEngine;
using System.Collections.Generic;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource; // Pour les SFX non spatialisés ou globaux

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1.0f; // Ce volume affectera aussi les SFX spatialisés via l'Audio Mixer

    [Header("Default Audio Clips (Optional)")]
    public AudioClip defaultMusicClip;
    public List<AudioClipWithID> sfxClips = new List<AudioClipWithID>();

    [System.Serializable]
    public class AudioClipWithID
    {
        public string id;
        public AudioClip clip;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;

        if (defaultMusicClip != null)
        {
            PlayMusic(defaultMusicClip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip) // Pour SFX non spatialisés
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(string id) // Pour SFX non spatialisés par ID
    {
        AudioClip clipToPlay = GetSFXClip(id);
        if (clipToPlay != null)
        {
            PlaySFX(clipToPlay);
        }
        else
        {
            Debug.LogWarning($"AudioController: SFX avec l'ID '{id}' non trouvé.");
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    private AudioClip GetSFXClip(string id)
    {
        foreach (var sfx in sfxClips)
        {
            if (sfx.id == id)
            {
                return sfx.clip;
            }
        }
        return null;
    }
}