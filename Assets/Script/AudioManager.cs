using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    // Permet d'accéder à l'AudioManager depuis n'importe quel script avec AudioManager.Instance
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Le haut-parleur pour les effets sonores courts.")]
    public AudioSource sfxSource;
    [Tooltip("Le haut-parleur pour la musique de fond.")]
    public AudioSource musicSource;

    [Header("Musique")]
    public AudioClip mainMenuMusic;
    public AudioClip gameMusic;

    [Header("Effets Sonores du Joueur")]
    public AudioClip weaponShootSound;
    public AudioClip pokeballThrowSound;
    public AudioClip jumpSound;
    public AudioClip pickupSound;

    [Header("Effets Sonores du Monde")]
    public AudioClip impactSound; // Quand un projectile touche quelque chose
    public AudioClip captureSuccessSound;
    public AudioClip captureFailSound;

    [Header("Effets Sonores de l'UI")]
    public AudioClip uiClickPlay;
    public AudioClip uiClickQuit;

    void Awake()
    {
        // --- Mise en place du Singleton ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Ne pas détruire cet objet en changeant de scène
        }
        else
        {
            Destroy(gameObject); // Détruire les doublons
        }
    }

    // --- Fonctions publiques pour jouer les sons ---

    public void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }
}