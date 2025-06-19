using Engine.Utils;
using UnityEngine;

public class InventoryManager : Singleton<InventoryManager>
{
    // Instance statique pour un acc�s facile de n'importe o�
    public new static InventoryManager Instance { get; private set; }

    // �tat de l'inventaire que nous voulons conserver
    public bool HasWeaponEquipped { get; set; } = false;
    public bool HasPokeballEquipped { get; set; } = false;

    protected override void Awake()
    {
        // Impl�mentation du Singleton:
        // S'assure qu'il n'y a qu'une seule instance de cet InventoryManager
        if (Instance == null)
        {
            Instance = this;
            // C'est la ligne magique ! Emp�che cet objet d'�tre d�truit lors des changements de sc�ne.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si une autre instance existe d�j�, d�truit cette nouvelle instance
            // (cela peut arriver si tu as un InventoryManager dans chaque sc�ne)
            Destroy(gameObject);
        }
    }

    // --- M�thodes pour ajouter/retirer des objets (utilis�es par le joueur quand il ramasse) ---

    public void AddWeapon()
    {
        HasWeaponEquipped = true;
        Debug.Log("Arme ajout�e � l'inventaire.");
    }

    public void RemoveWeapon()
    {
        HasWeaponEquipped = false;
        Debug.Log("Arme retir�e de l'inventaire.");
    }

    public void AddPokeball()
    {
        HasPokeballEquipped = true;
        Debug.Log("Pokeball ajout�e � l'inventaire.");
    }

    public void RemovePokeball()
    {
        HasPokeballEquipped = false;
        Debug.Log("Pokeball retir�e de l'inventaire.");
    }

    /// <summary>
    /// R�initialise compl�tement l'inventaire. Utile pour un "Nouvelle Partie" ou "Game Over".
    /// </summary>
    public void ResetInventory()
    {
        HasWeaponEquipped = false;
        HasPokeballEquipped = false;
        Debug.Log("Inventaire r�initialis�.");
    }
}