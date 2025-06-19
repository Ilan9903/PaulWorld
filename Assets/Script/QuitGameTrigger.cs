using UnityEngine;
using TMPro; // On ajoute ceci pour g�rer le texte d'aide

public class QuitGameTrigger : MonoBehaviour
{
    [Header("UI Prompt")]
    [Tooltip("Le texte qui s'affiche pour dire au joueur d'appuyer sur E.")]
    public TextMeshProUGUI quitPromptText;

    private bool playerIsInRange = false;

    void Start()
    {
        // On s'assure que le message est bien cach� au d�marrage
        if (quitPromptText != null)
        {
            quitPromptText.enabled = false;
        }
    }

    void Update()
    {
        // Si le joueur est dans la zone et appuie sur E
        if (playerIsInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Le joueur a demand� � quitter le jeu...");

            // Cette ligne ne fonctionne que dans un jeu "build�" (export�)
            Application.Quit();

            // Si on est dans l'�diteur Unity, on arr�te le mode Play
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // On v�rifie si c'est bien le joueur qui entre dans la zone
        if (other.CompareTag("Player"))
        {
            playerIsInRange = true;
            // On affiche le message d'aide
            if (quitPromptText != null)
            {
                quitPromptText.enabled = true;
            }
            Debug.Log("Entr� dans la zone de sortie. Appuyer sur E pour quitter.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Quand le joueur quitte la zone
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            // On cache le message d'aide
            if (quitPromptText != null)
            {
                quitPromptText.enabled = false;
            }
            Debug.Log("Sorti de la zone de sortie.");
        }
    }
}