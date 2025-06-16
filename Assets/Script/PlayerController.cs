using UnityEngine;

// On n'a plus besoin de "using UnityEngine.InputSystem;"
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --- Variables pour le mouvement ---
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;

    // --- Variables pour la cam�ra ---
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f; // Limite pour regarder en haut/bas

    private float rotationX = 0;

    // --- Variables pour l'arme ---
    public Transform weaponHolder;
    public GameObject projectilePrefab;
    public Transform firePoint;
    private bool hasWeapon = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Bloque le curseur au centre de l'�cran et le cache
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouvement du joueur (avec Input.GetAxis) ---
        float moveX = Input.GetAxis("Horizontal"); // Q/D ou A/D
        float moveZ = Input.GetAxis("Vertical");   // Z/S ou W/S

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // --- Gravit� ---
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // --- Rotation de la cam�ra (Mouse Look avec Input.GetAxis) ---
        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);

        // --- Gestion du tir ---
        // Si on a l'arme et qu'on clique
        if (hasWeapon && Input.GetButtonDown("Fire1")) // "Fire1" est le clic gauche par d�faut
        {
            HandleShooting();
        }
    }

    void HandleShooting()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // On cr�e une nouvelle instance du projectile � la position et rotation du firePoint
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        }
        else
        {
            Debug.LogError("Projectile Prefab ou Fire Point non assign� !");
        }
    }

    // --- Ramassage de l'arme (ne change pas) ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            Debug.Log("Arme ramass�e !");
            hasWeapon = true;

            other.transform.SetParent(weaponHolder);
            other.transform.localPosition = Vector3.zero;
            other.transform.localRotation = Quaternion.identity;

            // On d�sactive son trigger pour qu'elle devienne un objet solide attach� au joueur
            other.GetComponent<Collider>().enabled = false;

            other.gameObject.name = "ArmeEquipee";
        }
    }
}