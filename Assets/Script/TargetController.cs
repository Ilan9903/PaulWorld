using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float moveDistance = 5.0f; // Distance de déplacement de chaque côté

    // On rend la référence au GameManager publique pour la lier dans l'inspecteur
    public GameManager gameManager;

    private Vector3 startPosition;
    private bool isHit = false; // Pour s'assurer qu'on ne la touche qu'une fois

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Fait bouger la cible de gauche à droite en "ping-pong"
        float newX = startPosition.x + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    // Détecte la collision avec le projectile
    private void OnCollisionEnter(Collision collision)
    {
        // On vérifie que la cible n'a pas déjà été touchée et que l'objet est bien un projectile
        if (!isHit && collision.gameObject.CompareTag("Projectile"))
        {
            isHit = true;

            // On notifie le GameManager qu'une cible a été touchée
            if (gameManager != null)
            {
                gameManager.TargetHit();
            }

            // On désactive la cible
            gameObject.SetActive(false);
        }
    }
}