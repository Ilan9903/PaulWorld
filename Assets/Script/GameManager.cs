using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int scoreToWin = 3;
    public GameObject winScreen; // R�f�rence � notre �cran de victoire

    private int currentScore = 0;

    void Start()
    {
        // On s'assure que l'�cran de victoire est cach� au d�but
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
    }

    // Cette fonction sera appel�e par chaque cible lorsqu'elle est touch�e
    public void TargetHit()
    {
        currentScore++;
        Debug.Log("Cible touch�e ! Score : " + currentScore);

        if (currentScore >= scoreToWin)
        {
            // On a gagn� !
            ShowWinScreen();
        }
    }

    void ShowWinScreen()
    {
        Debug.Log("VICTOIRE !");
        if (winScreen != null)
        {
            winScreen.SetActive(true);

            // On lib�re le curseur de la souris pour pouvoir cliquer sur l'UI
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}