using UnityEngine;
using System.Collections;

public class PokeballProjectile : MonoBehaviour
{
    // Cette variable recevra la référence au prefab du pickup depuis le PlayerController
    public GameObject pokeballPickupPrefab;

    public float timeBeforeRespawn = 2.0f;
    private bool hasHit = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        PokemonController creature = collision.gameObject.GetComponent<PokemonController>();
        if (creature != null)
        {
            creature.AttemptToCapture();
        }

        // On lance la coroutine qui gère la transformation
        StartCoroutine(TransformIntoPickup());
    }

    private IEnumerator TransformIntoPickup()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // On arrête la vitesse AVANT de rendre l'objet kinematic
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // On attend X secondes
        yield return new WaitForSeconds(timeBeforeRespawn);

        // On fait réapparaître le pickup si la référence existe
        if (pokeballPickupPrefab != null)
        {
            Instantiate(pokeballPickupPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("La référence au pokeballPickupPrefab est manquante ! Vérifie l'inspecteur du Player.");
        }
        
        // On détruit le projectile (la sphère lancée)
        Destroy(gameObject);
    }
}