using UnityEngine;

public class PokeballProjectile : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        // On vérifie si on a touché une créature en cherchant son script
        PokemonController creature = collision.gameObject.GetComponent<PokemonController>();
        if (creature != null)
        {
            // Si c'est bien une créature, on appelle SA fonction pour tenter la capture
            creature.AttemptToCapture();
        }
        
        // Dans tous les cas, la sphère se détruit après avoir touché quelque chose
        Destroy(gameObject);
    }
}