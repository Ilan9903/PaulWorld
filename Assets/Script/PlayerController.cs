using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT ---
    [Header("Player Movement")]
    public float moveSpeed = 5.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -20.0f; // Une gravité un peu plus forte est souvent mieux pour le feeling
    private CharacterController controller;
    private Vector3 velocity;

    // --- VUE FPS ---
    [Header("Camera Look")]
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 80.0f;
    private float rotationX = 0;

    // --- ÉQUIPEMENT ---
    [Header("Equipment & Hands")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public Transform firePoint; // LE SEUL ET UNIQUE POINT DE TIR/LANCER

    [Header("Item Prefabs")]
    public GameObject pokeballPickupPrefab;
    public GameObject weaponVisualPrefab;   // Le MODÈLE 3D de l'arme
    public GameObject pokeballVisualPrefab; // Le MODÈLE 3D de la sphère
    public GameObject weaponProjectilePrefab; // La "BALLE" que l'arme tire
    public GameObject pokeballProjectilePrefab; // La "SPHÈRE" qui est lancée

    [Header("Item Stats")]
    public float throwForce = 15f;
    public float fireRate = 0.5f; // Temps en secondes entre chaque tir
    private float nextFireTime = 0f;

    // --- GESTION D'ÉTAT (privé) ---
    private GameObject equippedWeaponVisual;
    private GameObject equippedPokeballVisual;
    private bool hasWeapon = false;
    private bool hasPokeballInHand = false;

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
    }

    void HandleMovement()
    {
        // Gère la gravité
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Gère le saut (un seul bloc de condition pour isGrounded)
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Applique le mouvement (ZQSD) et la gravité/saut
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move((move * moveSpeed + velocity) * Time.deltaTime);

        // Gère la vue à la souris
        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeed; // -= pour un contrôle standard
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.Rotate(Vector3.up * lookY);
    }

    void HandleInput()
    {
        // Rappel: Fire1 = Clic Gauche, Fire2 = Clic Droit
        if (hasWeapon && hasPokeballInHand) // ÉTAT 3 : DEUX MAINS
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime) { ShootWeapon(); }
            if (Input.GetButtonDown("Fire2")) { ThrowPokeball(); }
        }
        else if (hasWeapon) // ÉTAT 1 : ARME SEULE
        {
            if (Input.GetButton("Fire1") && Time.time >= nextFireTime) { ShootWeapon(); }
        }
        else if (hasPokeballInHand) // ÉTAT 2 : SPHÈRE SEULE
        {
            if (Input.GetButtonDown("Fire1")) { ThrowPokeball(); }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            hasWeapon = true;
            Destroy(other.gameObject);
            UpdateHandVisuals(); // Appelle le chef d'orchestre
        }
        else if (other.CompareTag("PokeballPickup") && !hasPokeballInHand)
        {
            hasPokeballInHand = true;
            Destroy(other.gameObject);
            UpdateHandVisuals(); // Appelle le chef d'orchestre
        }
    }

    // --- FONCTION CENTRALE "CHEF D'ORCHESTRE" ---
    void UpdateHandVisuals()
    {
        // 1. On détruit les anciens objets pour nettoyer les mains
        if (equippedWeaponVisual != null) Destroy(equippedWeaponVisual);
        if (equippedPokeballVisual != null) Destroy(equippedPokeballVisual);

        // 2. On regarde l'état actuel et on recrée les bons objets aux bons endroits
        if (hasWeapon && hasPokeballInHand) // ÉTAT 3 : Les deux
        {
            equippedWeaponVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            equippedPokeballVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            firePoint.SetParent(leftHandHolder, false); // Le firePoint est sur l'arme
        }
        else if (hasWeapon) // ÉTAT 1 : Arme seule
        {
            equippedWeaponVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            firePoint.SetParent(rightHandHolder, false); // Le firePoint est sur l'arme
        }
        else if (hasPokeballInHand) // ÉTAT 2 : Sphère seule
        {
            equippedPokeballVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            firePoint.SetParent(rightHandHolder, false); // Le firePoint est sur la sphère
        }
        
        // 3. On réinitialise la position locale du firePoint pour qu'il soit bien au centre de la main
        firePoint.localPosition = Vector3.zero;
        firePoint.localRotation = Quaternion.identity;
    }

    void ShootWeapon()
    {
        Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
        nextFireTime = Time.time + fireRate; // Applique le délai
    }

    void ThrowPokeball()
    {
        if (pokeballProjectilePrefab == null) return;

        GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);

        // --- AJOUTE CETTE LIGNE ---
        // On dit au projectile qu'on vient de lancer quel est le pickup qu'il devra faire réapparaître.
        ball.GetComponent<PokeballProjectile>().pokeballPickupPrefab = this.pokeballPickupPrefab;
        // -------------------------

        ball.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

        hasPokeballInHand = false;
        UpdateHandVisuals();
    }
}