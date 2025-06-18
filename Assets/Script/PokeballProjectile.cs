using UnityEngine;
using System.Collections;

public class PokeballProjectile : MonoBehaviour
{
    // Référence au prefab du pickup, assignée par le PlayerController
    public GameObject pokeballPickupPrefab;

    // Temps d'attente au sol avant de se transformer en pickup
    public float timeBeforeRespawn = 2.0f;

    private bool hasHit = false;

    // La fonction Start est maintenant VIDE.
    // PAS DE "Destroy(gameObject, lifeTime);" ICI !
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Si on a déjà touché quelque chose, on ignore les autres collisions (ex: rebonds)
        if (hasHit) return;
        hasHit = true;

        // On tente la capture si on a touché une créature
        PokemonController creature = collision.gameObject.GetComponent<PokemonController>();
        if (creature != null)
        {
            creature.AttemptToCapture();
        }

        // On lance la coroutine qui gère le reste
        StartCoroutine(TransformIntoPickup());
    }

    private IEnumerator TransformIntoPickup()
    {
        // Arrête le mouvement de la sphère
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // Attend X secondes
        yield return new WaitForSeconds(timeBeforeRespawn);
        Debug.Log("Attente terminée, je vais créer le pickup !");

        // Fait apparaître le pickup
        if (pokeballPickupPrefab != null)
        {
            Instantiate(pokeballPickupPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("La référence au pokeballPickupPrefab est manquante sur le projectile !");
        }

        // Se détruit LUI-MÊME, à la toute fin
        Destroy(gameObject);
    }
}