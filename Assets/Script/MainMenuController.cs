using UnityEngine;
using UnityEngine.SceneManagement; // Indispensable pour changer de scène

public class MainMenuController : MonoBehaviour
{
    // Remplis ce nom de scène dans l'Inspecteur
    public string gameSceneName = "Scene 1"; // IMPORTANT : Mets ici le nom exact de ta scène de jeu

    // Cette fonction sera appelée par le bouton "JOUER"
    public void PlayGame()
    {
        // Charge la scène de jeu
        SceneManager.LoadScene(gameSceneName);
    }

    // Cette fonction sera appelée par le bouton "QUITTER"
    public void QuitGame()
    {
        // Ce message s'affichera dans la console de Unity
        Debug.Log("Le joueur a quitté le jeu.");
        
        // Cette commande ne fonctionne que dans un jeu "buildé" (exporté), pas dans l'éditeur.
        Application.Quit();
    }
}