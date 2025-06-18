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
    public GameObject weaponVisualPrefab;       // Le modèle 3D de l'arme
    public GameObject pokeballVisualPrefab;     // Le modèle 3D de la sphère
    public GameObject weaponProjectilePrefab;   // La "balle" tirée par l'arme
    public GameObject pokeballProjectilePrefab; // La sphère à lancer

    [Header("Item Stats")]
    public float throwForce = 15f;
    public float fireRate = 0.5f; // 2 tirs par seconde
    private float nextFireTime = 0f;

    // --- GESTION D'ÉTAT ---
    private GameObject currentRightHandItemVisual = null; // Store the actual GameObject in the right hand
    private GameObject currentLeftHandItemVisual = null;  // Store the actual GameObject in the left hand

    private bool hasWeaponEquipped = false;
    private bool hasPokeballEquipped = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Ensure firePoint starts detached if not specified in editor or moved by a hand
        // Position it slightly in front of the camera initially
        firePoint.SetParent(playerCamera.transform);
        firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f); // Adjust these values for proper visibility
        firePoint.localRotation = Quaternion.identity;
    }

    void Update()
    {
        HandleMovement();
        HandleInput();
    }

    // --- LOGIQUE DE DÉPLACEMENT ET DE VUE ---
    void HandleMovement()
    {
        // Apply gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        // Handle jump
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply movement and gravity
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime + velocity * Time.deltaTime);

        // Handle FPS view
        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);
    }

    // --- LOGIQUE D'ACTIONS (MISE À JOUR) ---
    void HandleInput()
    {
        // Weapon Logic (Left Click)
        // Check if a weapon is equipped and the fire rate allows firing
        if (hasWeaponEquipped && Time.time >= nextFireTime)
        {
            // We use Input.GetButton (not GetButtonDown) for continuous firing if held
            if (Input.GetButton("Fire1")) // Clic Gauche
            {
                ShootWeapon();
            }
        }

        // Pokeball Logic (Right Click)
        // Check if a pokeball is equipped
        if (hasPokeballEquipped)
        {
            // We use Input.GetButtonDown (not GetButton) for a single throw per click
            if (Input.GetButtonDown("Fire2")) // Clic Droit
            {
                ThrowPokeball();
            }
            // Special case: If ONLY the pokeball is equipped, use Left Click to throw
            else if (!hasWeaponEquipped && Input.GetButtonDown("Fire1")) // Clic Gauche quand seule la pokeball est là
            {
                ThrowPokeball();
            }
        }
    }

    // --- LOGIQUE DE RAMASSAGE ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WeaponPickup") && !hasWeaponEquipped)
        {
            Destroy(other.gameObject); // Destroy the pickup object
            hasWeaponEquipped = true;
            UpdateHandVisuals(); // Update visual representation
        }
        else if (other.CompareTag("PokeballPickup") && !hasPokeballEquipped)
        {
            Destroy(other.gameObject); // Destroy the pickup object
            hasPokeballEquipped = true;
            UpdateHandVisuals(); // Update visual representation
        }
    }

    // --- FONCTION CENTRALE "CHEF D'ORCHESTRE" ---
    void UpdateHandVisuals()
    {
        // 1. Clear current hands
        if (currentRightHandItemVisual != null)
        {
            Destroy(currentRightHandItemVisual);
            currentRightHandItemVisual = null;
        }
        if (currentLeftHandItemVisual != null)
        {
            Destroy(currentLeftHandItemVisual);
            currentLeftHandItemVisual = null;
        }

        // 2. Decide what to equip and where
        if (hasWeaponEquipped && hasPokeballEquipped) // Case: Both items held
        {
            // Weapon goes to left hand
            currentLeftHandItemVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            currentLeftHandItemVisual.tag = "Weapon"; // Ensure visual also has tag
            // Adjust these values for weapon positioning in the left hand
            currentLeftHandItemVisual.transform.localPosition = Vector3.zero;
            currentLeftHandItemVisual.transform.localRotation = Quaternion.identity;

            // Pokeball goes to right hand
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball"; // Ensure visual also has tag
            // Adjust these values for pokeball positioning in the right hand
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Fire point follows the weapon (left hand)
            firePoint.SetParent(leftHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust relative to weapon
            firePoint.localRotation = Quaternion.identity;
        }
        else if (hasWeaponEquipped && !hasPokeballEquipped) // Case: Only weapon held
        {
            // Weapon goes to right hand (as it's the only item)
            currentRightHandItemVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Weapon";
            // Adjust these values for weapon positioning in the right hand
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Fire point follows the weapon (right hand)
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust relative to weapon
            firePoint.localRotation = Quaternion.identity;
        }
        else if (!hasWeaponEquipped && hasPokeballEquipped) // Case: Only Pokeball held
        {
            // Pokeball goes to right hand (as it's the only item)
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            // Adjust these values for pokeball positioning in the right hand
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Fire point follows the pokeball (right hand)
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust relative to pokeball
            firePoint.localRotation = Quaternion.identity;
        }
        else // No items equipped
        {
            // Ensure fire point is in a neutral position
            firePoint.SetParent(playerCamera.transform);
            firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f); // Initial position relative to camera
            firePoint.localRotation = Quaternion.identity;
        }

        // Tip: Adjust localPosition and localRotation for each visual prefab in the editor
        // after instantiating it in a temporary scene to find the correct values,
        // then copy them here.
    }

    // --- Fonctions d'action ---
    void ShootWeapon()
    {
        // Ensure weapon is equipped
        if (hasWeaponEquipped)
        {
            // Instantiate projectile at firePoint's position and rotation
            Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate; // Apply fire rate delay
        }
    }

    void ThrowPokeball()
    {
        // Ensure pokeball is equipped
        if (hasPokeballEquipped)
        {
            // Instantiate pokeball at firePoint's position and rotation
            GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);

            // Ensure the projectile has a Rigidbody to apply force
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("The Pokeball Projectile Prefab needs a Rigidbody component!");
                Destroy(ball); // Destroy the object if it cannot be thrown
                return;
            }

            // Apply force to launch the pokeball in the camera's forward direction
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            // Once thrown, the pokeball is no longer "in hand"
            hasPokeballEquipped = false;
            UpdateHandVisuals(); // Update hand display
        }
    }
}