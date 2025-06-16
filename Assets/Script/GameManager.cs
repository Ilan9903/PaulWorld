using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int scoreToWin = 3;
    public GameObject winScreen; // Référence à notre écran de victoire

    private int currentScore = 0;

    void Start()
    {
        // On s'assure que l'écran de victoire est caché au début
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
    }

    // Cette fonction sera appelée par chaque cible lorsqu'elle est touchée
    public void TargetHit()
    {
        currentScore++;
        Debug.Log("Cible touchée ! Score : " + currentScore);

        if (currentScore >= scoreToWin)
        {
            // On a gagné !
            ShowWinScreen();
        }
    }

    void ShowWinScreen()
    {
        Debug.Log("VICTOIRE !");
        if (winScreen != null)
        {
            winScreen.SetActive(true);

            // On libère le curseur de la souris pour pouvoir cliquer sur l'UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}