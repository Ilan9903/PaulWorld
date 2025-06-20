using UnityEngine;
using UnityEngine.SceneManagement; // Indispensable pour changer de scène

public class MainMenuController : MonoBehaviour
{
    // Remplis ce nom de scène dans l'Inspecteur
    public string gameSceneName = "Scene 1"; // IMPORTANT : Mets ici le nom exact de ta scène de jeu

    // Cette fonction sera appelée par le bouton "JOUER"
    public void PlayGame()
    {
        AudioManager.Instance.PlaySound(AudioManager.Instance.uiClickPlay);
        AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
        // Charge la scène de jeu
        SceneManager.LoadScene(gameSceneName);
    }

    // Cette fonction sera appelée par le bouton "QUITTER"
    public void QuitGame()
    {
        // Ce message s'affichera dans la console de Unity
        Debug.Log("Le joueur a quitté le jeu.");
        AudioManager.Instance.PlaySound(AudioManager.Instance.uiClickQuit);
        // Cette commande ne fonctionne que dans un jeu "buildé" (exporté), pas dans l'éditeur.
        Application.Quit();
    }

    void Start()
    {
        // On vérifie que l'AudioManager existe avant de jouer de la musique
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(AudioManager.Instance.mainMenuMusic);
        }
    }
}