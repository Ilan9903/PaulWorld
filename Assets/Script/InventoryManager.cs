using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // Instance statique pour un accès facile de n'importe où
    public static InventoryManager Instance { get; private set; }

    // État de l'inventaire que nous voulons conserver
    public bool HasWeaponEquipped { get; set; } = false;
    public bool HasPokeballEquipped { get; set; } = false;

    void Awake()
    {
        // Implémentation du Singleton:
        // S'assure qu'il n'y a qu'une seule instance de cet InventoryManager
        if (Instance == null)
        {
            Instance = this;
            // C'est la ligne magique ! Empêche cet objet d'être détruit lors des changements de scène.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si une autre instance existe déjà, détruit cette nouvelle instance
            // (cela peut arriver si tu as un InventoryManager dans chaque scène)
            Destroy(gameObject);
        }
    }

    // --- Méthodes pour ajouter/retirer des objets (utilisées par le joueur quand il ramasse) ---

    public void AddWeapon()
    {
        HasWeaponEquipped = true;
        Debug.Log("Arme ajoutée à l'inventaire.");
    }

    public void RemoveWeapon()
    {
        HasWeaponEquipped = false;
        Debug.Log("Arme retirée de l'inventaire.");
    }

    public void AddPokeball()
    {
        HasPokeballEquipped = true;
        Debug.Log("Pokeball ajoutée à l'inventaire.");
    }

    public void RemovePokeball()
    {
        HasPokeballEquipped = false;
        Debug.Log("Pokeball retirée de l'inventaire.");
    }

    /// <summary>
    /// Réinitialise complètement l'inventaire. Utile pour un "Nouvelle Partie" ou "Game Over".
    /// </summary>
    public void ResetInventory()
    {
        HasWeaponEquipped = false;
        HasPokeballEquipped = false;
        Debug.Log("Inventaire réinitialisé.");
    }
}