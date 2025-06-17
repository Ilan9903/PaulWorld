using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int level = 1;
    public int currentXp = 0;
    public int xpToNextLevel = 100;

    public int captureSpheres = 5; // Commence avec 5 sphères
    public int money = 0;

    // Fonction pour ajouter de l'XP
    public void AddXp(int amount)
    {
        currentXp += amount;
        // Logique pour monter de niveau si currentXp >= xpToNextLevel
    }

    // Fonctions pour gérer les sphères
    public bool HasSpheres() { return captureSpheres > 0; }
    public void UseSphere() { captureSpheres--; }
}