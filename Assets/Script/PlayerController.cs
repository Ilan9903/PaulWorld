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
        firePoint.SetParent(playerCamera.transform); // Attach to camera initially or a neutral point
        firePoint.localPosition = Vector3.forward * 0.5f; // Example: slightly in front of camera
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

    // --- LOGIQUE D'ACTIONS ---
    void HandleInput()
    {
        if (Time.time < nextFireTime) return;

        // Check for weapon in left hand (preferred for shooting when both are held)
        if (currentLeftHandItemVisual != null && currentLeftHandItemVisual.CompareTag("Weapon"))
        {
            if (Input.GetButton("Fire1")) // Clic Gauche : TIRE
            {
                ShootWeapon();
            }
        }
        // Check for weapon in right hand (when only weapon is held or weapon is first picked up)
        else if (currentRightHandItemVisual != null && currentRightHandItemVisual.CompareTag("Weapon"))
        {
            if (Input.GetButton("Fire1")) // Clic Gauche : TIRE
            {
                ShootWeapon();
            }
        }

        // Check for pokeball in right hand
        if (currentRightHandItemVisual != null && currentRightHandItemVisual.CompareTag("Pokeball"))
        {
            if (Input.GetButtonDown("Fire2")) // Clic Droit : LANCE
            {
                ThrowPokeball();
            }
        }
        // If only pokeball is held, it's in the right hand, so it should fire on left click
        else if (hasPokeballEquipped && !hasWeaponEquipped)
        {
            if (Input.GetButtonDown("Fire1")) // Clic Gauche : LANCE (quand seule la pokeball est là)
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
            // Destroy the pickup object
            Destroy(other.gameObject);
            hasWeaponEquipped = true;
            UpdateHandVisuals(); // Update visual representation
        }
        else if (other.CompareTag("PokeballPickup") && !hasPokeballEquipped)
        {
            // Destroy the pickup object
            Destroy(other.gameObject);
            hasPokeballEquipped = true;
            UpdateHandVisuals(); // Update visual representation
        }
    }

    // --- NOUVELLE FONCTION CENTRALE "CHEF D'ORCHESTRE" ---
    void UpdateHandVisuals()
    {
        // 1. Dégager les mains actuelles
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

        // 2. Décider quoi équiper et où
        if (hasWeaponEquipped && hasPokeballEquipped) // Case: Both items held
        {
            // Weapon goes to left hand
            currentLeftHandItemVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            currentLeftHandItemVisual.tag = "Weapon"; // Ensure visual also has tag for input check
            currentLeftHandItemVisual.transform.localPosition = Vector3.zero; // Adjust as needed
            currentLeftHandItemVisual.transform.localRotation = Quaternion.identity; // Adjust as needed

            // Pokeball goes to right hand
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball"; // Ensure visual also has tag for input check
            currentRightHandItemVisual.transform.localPosition = Vector3.zero; // Adjust as needed
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity; // Adjust as needed

            // Fire point follows the weapon
            firePoint.SetParent(leftHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust as needed relative to weapon
        }
        else if (hasWeaponEquipped && !hasPokeballEquipped) // Case: Only weapon held
        {
            // Weapon goes to right hand (as it's the only item)
            currentRightHandItemVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Weapon";
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Fire point follows the weapon
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust as needed relative to weapon
        }
        else if (!hasWeaponEquipped && hasPokeballEquipped) // Case: Only Pokeball held
        {
            // Pokeball goes to right hand (as it's the only item)
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Fire point follows the pokeball
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Adjust as needed relative to pokeball
        }
        else // No items equipped
        {
            // Ensure fire point is in a neutral position if no items are held
            firePoint.SetParent(playerCamera.transform);
            firePoint.localPosition = Vector3.forward * 0.5f; // Example: slightly in front of camera
            firePoint.localRotation = Quaternion.identity;
        }

        // You might want to adjust localPosition and localRotation for each visual prefab
        // to make them look good in the hand. For example:
        // if (currentRightHandItemVisual != null) currentRightHandItemVisual.transform.localPosition = new Vector3(0.1f, -0.1f, 0.3f);
    }

    // --- Fonctions d'action ---
    void ShootWeapon()
    {
        // Ensure the weapon visual exists and is correctly identified
        if (hasWeaponEquipped)
        {
            Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate; // Applique le délai
        }
    }

    void ThrowPokeball()
    {
        // Ensure pokeball visual exists and is correctly identified
        if (hasPokeballEquipped)
        {
            GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);
            // Ensure the projectile has a Rigidbody to apply force
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Pokeball Projectile Prefab needs a Rigidbody component!");
                Destroy(ball); // Destroy if it can't be thrown
                return;
            }
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            // Once thrown, the pokeball is no longer "in hand"
            hasPokeballEquipped = false;
            UpdateHandVisuals(); // Update the display
        }
    }
}