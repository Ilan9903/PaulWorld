using UnityEngine;
using System.Collections; // Indispensable pour utiliser les Coroutines

public class PokeballProjectile : MonoBehaviour
{
    // Variable publique pour recevoir la référence du pickup depuis le PlayerController
    public GameObject pokeballPickupPrefab;

    public float timeBeforeRespawn = 2.0f; // Temps en secondes avant que le pickup réapparaisse

    private bool hasHit = false; // Un drapeau pour s'assurer que la logique ne s'exécute qu'une fois

    private void OnCollisionEnter(Collision collision)
    {
        // Si on a déjà touché quelque chose, on ne fait plus rien
        if (hasHit) return;
        
        hasHit = true; // On lève le drapeau

        // On tente de capturer si on touche une créature
        PokemonController creature = collision.gameObject.GetComponent<PokemonController>();
        if (creature != null)
        {
            creature.AttemptToCapture();
        }

        // On lance la coroutine qui va gérer la transformation en pickup
        StartCoroutine(TransformIntoPickup());
    }

    private IEnumerator TransformIntoPickup()
    {
        // 1. On arrête la physique pour que la sphère arrête de rouler
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // 2. On attend X secondes
        yield return new WaitForSeconds(timeBeforeRespawn);

        // 3. On fait apparaître le pickup à l'endroit où la sphère a atterri
        if (pokeballPickupPrefab != null)
        {
            Instantiate(pokeballPickupPrefab, transform.position, Quaternion.identity);
        }

        // 4. On détruit l'objet projectile (la sphère lancée)
        Destroy(gameObject);
    }
}