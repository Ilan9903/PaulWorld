using UnityEngine;

public class TargetController : MonoBehaviour
{
    public float moveSpeed = 2.0f;
    public float moveDistance = 5.0f; // Distance de d�placement de chaque c�t�

    // On rend la r�f�rence au GameManager publique pour la lier dans l'inspecteur
    public GameManager gameManager;

    private Vector3 startPosition;
    private bool isHit = false; // Pour s'assurer qu'on ne la touche qu'une fois

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Fait bouger la cible de gauche � droite en "ping-pong"
        float newX = startPosition.x + Mathf.PingPong(Time.time * moveSpeed, moveDistance * 2) - moveDistance;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    // D�tecte la collision avec le projectile
    private void OnCollisionEnter(Collision collision)
    {
        // On v�rifie que la cible n'a pas d�j� �t� touch�e et que l'objet est bien un projectile
        if (!isHit && collision.gameObject.CompareTag("Projectile"))
        {
            isHit = true;

            // On notifie le GameManager qu'une cible a �t� touch�e
            if (gameManager != null)
            {
                gameManager.TargetHit();
            }

            // On d�sactive la cible
            gameObject.SetActive(false);
        }
    }
}