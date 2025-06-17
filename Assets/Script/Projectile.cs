using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 3f;
    public float damage = 10f; // Dégâts du projectile

    void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Cherche si l'objet touché est une créature
        PokemonController creature = collision.gameObject.GetComponent<PokemonController>();
        if (creature != null)
        {
            // Si oui, inflige des dégâts
            creature.TakeDamage(damage);
        }

        // Le projectile se détruit à l'impact
        Destroy(gameObject);
    }
}