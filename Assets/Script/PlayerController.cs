using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT ---
    [Header("Player Movement")]
    public float moveSpeed = 5.0f;
    public float jumpHeight = 2.0f;
    public float gravity = -9.81f;
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
    public GameObject weaponVisualPrefab;   // Le modèle 3D de l'arme
    public GameObject pokeballVisualPrefab; // Le modèle 3D de la sphère
    public GameObject weaponProjectilePrefab; // La "balle" tirée par l'arme
    public GameObject pokeballProjectilePrefab; // La sphère à lancer

    [Header("Item Stats")]
    public float throwForce = 15f;
    public float fireRate = 0.5f; // 2 tirs par seconde
    private float nextFireTime = 0f;

    // --- GESTION D'ÉTAT ---
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

    // --- LOGIQUE DE DÉPLACEMENT ET DE VUE (corrigée) ---
    void HandleMovement()
    {
        // Applique la gravité
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Gère le saut
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Applique le mouvement et la gravité
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime + velocity * Time.deltaTime);

        // Gère la vue FPS
        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);
    }

    // --- LOGIQUE D'ACTIONS (inchangée) ---
    void HandleInput()
    {
        if (Time.time < nextFireTime) return;

        if (hasWeapon && hasPokeballInHand)
        {
            if (Input.GetButton("Fire1")) { ShootWeapon(); }      // Clic Gauche : TIRE
            if (Input.GetButtonDown("Fire2")) { ThrowPokeball(); }    // Clic Droit : LANCE
        }
        else if (hasWeapon)
        {
            if (Input.GetButton("Fire1")) { ShootWeapon(); }      // Clic Gauche : TIRE
        }
        else if (hasPokeballInHand)
        {
            if (Input.GetButtonDown("Fire1")) { ThrowPokeball(); }    // Clic Gauche : LANCE
        }
    }

    // --- LOGIQUE DE RAMASSAGE (simplifiée) ---
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

    // --- NOUVELLE FONCTION CENTRALE "CHEF D'ORCHESTRE" ---
    void UpdateHandVisuals()
    {
        // 1. On détruit les anciens objets visuels pour éviter les doublons
        if (equippedWeaponVisual != null) Destroy(equippedWeaponVisual);
        if (equippedPokeballVisual != null) Destroy(equippedPokeballVisual);

        // 2. On regarde l'état actuel et on instancie les nouveaux objets visuels
        if (hasWeapon && hasPokeballInHand) // ÉTAT 3 : DEUX MAINS
        {
            equippedWeaponVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            equippedPokeballVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            firePoint.SetParent(leftHandHolder); // Le firePoint va avec l'arme
        }
        else if (hasWeapon) // ÉTAT 1 : ARME SEULE
        {
            equippedWeaponVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            firePoint.SetParent(rightHandHolder); // Le firePoint va avec l'arme
        }
        else if (hasPokeballInHand) // ÉTAT 2 : SPHÈRE SEULE
        {
            equippedPokeballVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            firePoint.SetParent(rightHandHolder); // Le firePoint va avec la sphère
        }
        
        // 3. On réinitialise la position locale du firePoint pour qu'il soit bien au centre de la main
        firePoint.localPosition = Vector3.zero;
    }

    // --- Fonctions d'action ---
    void ShootWeapon()
    {
        Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
        nextFireTime = Time.time + fireRate; // Applique le délai
    }

    void ThrowPokeball()
    {
        GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);
        ball.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

        hasPokeballInHand = false;
        UpdateHandVisuals(); // Appelle le chef d'orchestre pour mettre à jour l'affichage
    }
}