using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifeTime = 3f; // Dur�e de vie en secondes

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // On propulse le projectile vers l'avant d�s sa cr�ation
        rb.linearVelocity = transform.forward * speed;

        // On le d�truit apr�s un certain temps pour ne pas polluer la sc�ne
        Destroy(gameObject, lifeTime);
    }

    // Optionnel : G�rer les collisions
    private void OnCollisionEnter(Collision collision)
    {
        // On peut ajouter des effets ici (explosion, etc.)
        Debug.Log("Le projectile a touch� : " + collision.gameObject.name);
        Destroy(gameObject); // D�truit le projectile � l'impact
    }
}