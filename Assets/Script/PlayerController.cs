using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT (votre code existant) ---
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    private float rotationX = 0;

    // --- NOUVEAU : GESTION DES MAINS ET ÉQUIPEMENT ---
    [Header("Hand & Equipment")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public Transform throwPoint; // Point de départ pour la sphère lancée

    [Header("Prefabs")]
    public GameObject weaponInHandPrefab; // Le modèle 3D de l'arme à tenir
    public GameObject pokeballInHandPrefab; // Le modèle de la sphère à tenir

    private GameObject equippedWeapon;
    private GameObject equippedPokeball;

    // États du joueur
    private bool hasWeapon = false;
    private bool hasPokeball = false; // Note : on considère qu'on ne peut avoir une pokeball que si on a déjà une arme

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- GESTION DU MOUVEMENT ---
        HandleMovement(); // Votre code de mouvement est ici

        // --- GESTION DES ACTIONS ---
        HandleInput();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0) { velocity.y = -2f; }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);
    }

    void HandleInput()
    {
        if (hasWeapon && hasPokeball)
        {
            // MODE 2 MAINS : Arme à gauche, Sphère à droite
            if (Input.GetButtonDown("Fire1")) { ShootWeapon(); } // Clic Gauche
            if (Input.GetButtonDown("Fire2")) { /* On fera ça au Jour 2 */ } // Clic Droit
        }
        else if (hasWeapon)
        {
            // MODE ARME SEULE : Arme à droite
            if (Input.GetButtonDown("Fire2")) { ShootWeapon(); } // Clic Droit
        }
    }

    void ShootWeapon()
    {
        Debug.Log("PAN ! (Logique de tir à implémenter)");
        // On créera le projectile à la Tâche 6
    }

    // Détecter les pickups
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            Debug.Log("Arme ramassée !");
            hasWeapon = true;
            // Instancier l'arme et la mettre dans la main droite
            equippedWeapon = Instantiate(weaponInHandPrefab, rightHandHolder.position, rightHandHolder.rotation, rightHandHolder);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PokeballPickup") && hasWeapon && !hasPokeball)
        {
            Debug.Log("Sphère de capture ramassée !");
            hasPokeball = true;
            // Instancier la sphère et la mettre dans la main droite
            equippedPokeball = Instantiate(pokeballInHandPrefab, rightHandHolder.position, rightHandHolder.rotation, rightHandHolder);

            // On déplace l'arme dans la main gauche
            equippedWeapon.transform.SetParent(leftHandHolder, false); // false pour garder la taille locale
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;

            Destroy(other.gameObject);
        }
    }
}