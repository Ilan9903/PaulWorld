using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT ---
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    private float rotationX = 0;

    // --- GESTION DES MAINS ET ÉQUIPEMENT ---
    [Header("Hand & Equipment")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public Transform throwPoint; // Point de départ pour la sphère lancée

    [Header("Prefabs Visuels")]
    public GameObject weaponInHandPrefab; // Le modèle 3D de l'arme à tenir
    public GameObject pokeballInHandPrefab; // Le modèle de la sphère à tenir
    public float throwForce = 10f;

    // AJOUTÉ : Header et variables manquantes pour le tir
    [Header("Weapon & Projectiles")]
    public GameObject weaponProjectilePrefab; // Le prefab de la balle/laser
    public Transform weaponFirePoint; // Un point au bout du canon de l'arme visuelle

    private GameObject equippedWeapon;
    private GameObject equippedPokeball;

    // États du joueur
    private bool hasWeapon = false;
    private bool hasPokeball = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleInput();
        if (Input.GetMouseButtonDown(1)) // Clic droit
        {
            ThrowPokeball();
        }
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
            if (Input.GetButtonDown("Fire1")) { ShootWeapon(); } // Clic Gauche
            if (Input.GetButtonDown("Fire2")) { /* On fera ça au Jour 2 */ } // Clic Droit
        }
        else if (hasWeapon)
        {
            if (Input.GetButtonDown("Fire2")) { ShootWeapon(); } // Clic Droit
        }
    }

    void ShootWeapon()
    {
        if (weaponProjectilePrefab == null)
        {
            Debug.LogError("Le prefab du projectile d'arme n'est pas assigné !");
            return;
        }

        // On utilise le point de tir de l'arme s'il est défini, sinon celui de la caméra par défaut
        Transform firePointToUse = (weaponFirePoint != null) ? weaponFirePoint : throwPoint;
        Instantiate(weaponProjectilePrefab, firePointToUse.position, firePointToUse.rotation);
    }
    
    void ThrowPokeball()
    {
        if (pokeballInHandPrefab != null && throwPoint != null)
        {
            GameObject pokeball = Instantiate(pokeballInHandPrefab, throwPoint.position, throwPoint.rotation);
            Rigidbody rb = pokeball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
            }
        }
        else
        {
            Debug.LogWarning("PokeballPrefab ou ThrowPoint non assigné !");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            Debug.Log("Arme ramassée !");
            hasWeapon = true;

            // CORRIGÉ : Méthode plus robuste pour instancier et parenter
            equippedWeapon = Instantiate(weaponInHandPrefab);
            equippedWeapon.transform.SetParent(rightHandHolder, false);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PokeballPickup") && hasWeapon && !hasPokeball)
        {
            Debug.Log("Sphère de capture ramassée !");
            hasPokeball = true;

            // CORRIGÉ : Méthode plus robuste pour instancier et parenter
            equippedPokeball = Instantiate(pokeballInHandPrefab);
            equippedPokeball.transform.SetParent(rightHandHolder, false);
            equippedPokeball.transform.localPosition = Vector3.zero;
            equippedPokeball.transform.localRotation = Quaternion.identity;

            // On déplace l'arme dans la main gauche
            equippedWeapon.transform.SetParent(leftHandHolder, false);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;

            Destroy(other.gameObject);
        }
    }
}