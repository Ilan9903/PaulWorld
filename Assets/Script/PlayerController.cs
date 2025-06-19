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
    public Transform firePoint;

    [Header("Item Prefabs")]
    public GameObject weaponVisualPrefab;
    public GameObject pokeballVisualPrefab;
    public GameObject weaponProjectilePrefab;
    public GameObject pokeballProjectilePrefab;
    public GameObject pokeballPickupPrefab;

    [Header("Item Stats")]
    public float throwForce = 15f;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    // --- GESTION D'ÉTAT ---
    private GameObject currentRightHandItemVisual = null;
    private GameObject currentLeftHandItemVisual = null;

    // Supprime ces variables, l'état est maintenant géré par InventoryManager
    // private bool hasWeaponEquipped = false;
    // private bool hasPokeballEquipped = false;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerCamera != null && firePoint != null)
        {
            firePoint.SetParent(playerCamera.transform);
            firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f);
            firePoint.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogError("PlayerController: playerCamera ou firePoint n'est pas assigné dans l'Inspecteur ou est manquant. Veuillez vérifier !");
        }

        // MISE À JOUR IMPORTANTE ICI:
        // Au démarrage de la scène, met à jour les visuels des mains en fonction de l'inventaire persistant
        if (InventoryManager.Instance != null)
        {
            UpdateHandVisuals();
        }
        else
        {
            Debug.LogError("InventoryManager.Instance est null. Assurez-vous d'avoir un GameObject avec le script InventoryManager dans votre première scène et qu'il est configuré pour DontDestroyOnLoad.");
        }
    }

    void Update()
    {
        HandleMovement();
        HandleInput();
    }

    // --- LOGIQUE DE DÉPLACEMENT ET DE VUE (aucune modification ici) ---
    void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * moveSpeed * Time.deltaTime + velocity * Time.deltaTime);

        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);
    }

    // --- LOGIQUE D'ACTIONS (MISE À JOUR) ---
    void HandleInput()
    {
        // On s'assure que l'InventoryManager existe avant d'essayer d'y accéder
        if (InventoryManager.Instance == null) return;

        // Logique de l'arme (Clic Gauche)
        // Utilise InventoryManager.Instance.HasWeaponEquipped
        if (InventoryManager.Instance.HasWeaponEquipped && Time.time >= nextFireTime)
        {
            if (Input.GetButton("Fire1"))
            {
                ShootWeapon();
            }
        }

        // Logique de la Pokeball (Clic Droit)
        // Utilise InventoryManager.Instance.HasPokeballEquipped
        if (InventoryManager.Instance.HasPokeballEquipped)
        {
            if (Input.GetButtonDown("Fire2"))
            {
                ThrowPokeball();
            }
            // Cas spécial : Si SEULEMENT la pokeball est équipée, utiliser le Clic Gauche
            else if (!InventoryManager.Instance.HasWeaponEquipped && Input.GetButtonDown("Fire1"))
            {
                ThrowPokeball();
            }
        }
    }

    // --- LOGIQUE DE RAMASSAGE (MISE À JOUR) ---
    private void OnTriggerEnter(Collider other)
    {
        // On s'assure que l'InventoryManager existe
        if (InventoryManager.Instance == null) return;

        if (other.CompareTag("WeaponPickup") && !InventoryManager.Instance.HasWeaponEquipped)
        {
            Destroy(other.gameObject);
            InventoryManager.Instance.AddWeapon(); // Informe l'inventaire qu'une arme est ajoutée
            UpdateHandVisuals();
        }
        else if (other.CompareTag("PokeballPickup") && !InventoryManager.Instance.HasPokeballEquipped)
        {
            Destroy(other.gameObject);
            InventoryManager.Instance.AddPokeball(); // Informe l'inventaire qu'une pokeball est ajoutée
            UpdateHandVisuals();
        }
    }

    // --- FONCTION CENTRALE "CHEF D'ORCHESTRE" (MISE À JOUR) ---
    void UpdateHandVisuals()
    {
        // On s'assure que l'InventoryManager existe avant d'y accéder
        if (InventoryManager.Instance == null) return;

        // Dégager les mains actuelles
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

        // Décider quoi équiper et où, en se basant sur l'InventoryManager
        bool hasWeapon = InventoryManager.Instance.HasWeaponEquipped;
        bool hasPokeball = InventoryManager.Instance.HasPokeballEquipped;

        if (hasWeapon && hasPokeball)
        {
            currentLeftHandItemVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            currentLeftHandItemVisual.tag = "Weapon";
            currentLeftHandItemVisual.transform.localPosition = Vector3.zero;
            currentLeftHandItemVisual.transform.localRotation = Quaternion.identity;

            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            if (firePoint != null)
            {
                firePoint.localPosition = leftHandHolder.localPosition + new Vector3(0.2f, 0, 0.5f);
                firePoint.localRotation = leftHandHolder.localRotation;
            }
        }
        else if (hasWeapon && !hasPokeball)
        {
            currentRightHandItemVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Weapon";
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            if (firePoint != null)
            {
                firePoint.localPosition = rightHandHolder.localPosition + new Vector3(-0.2f, 0, 0.5f);
                firePoint.localRotation = rightHandHolder.localRotation;
            }
        }
        else if (!hasWeapon && hasPokeball)
        {
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            if (firePoint != null)
            {
                firePoint.localPosition = rightHandHolder.localPosition + new Vector3(0, 0, 0.3f);
                firePoint.localRotation = rightHandHolder.localRotation;
            }
        }
        else // Aucun objet équipé
        {
            if (firePoint != null)
            {
                firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f);
                firePoint.localRotation = Quaternion.identity;
            }
        }
    }

    // --- Fonctions d'action (MISE À JOUR pour ThrowPokeball) ---
    void ShootWeapon()
    {
        // Utilise InventoryManager.Instance.HasWeaponEquipped
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasWeaponEquipped)
        {
            if (firePoint == null)
            {
                Debug.LogError("ShootWeapon: firePoint est null. Impossible de tirer. Vérifiez son assignation dans l'Inspecteur !");
                return;
            }
            if (weaponProjectilePrefab == null)
            {
                Debug.LogError("ShootWeapon: weaponProjectilePrefab n'est pas assigné !");
                return;
            }

            Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate;
        }
    }

    void ThrowPokeball()
    {
        // Utilise InventoryManager.Instance.HasPokeballEquipped
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasPokeballEquipped)
        {
            if (firePoint == null)
            {
                Debug.LogError("ThrowPokeball: firePoint est null. Impossible de lancer. Vérifiez son assignation dans l'Inspecteur !");
                return;
            }
            if (pokeballProjectilePrefab == null)
            {
                Debug.LogError("ThrowPokeball: pokeballProjectilePrefab n'est pas assigné !");
                return;
            }

            GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);
            ball.GetComponent<PokeballProjectile>().pokeballPickupPrefab = this.pokeballPickupPrefab;
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Le préfabriqué de la Pokeball Projectile a besoin d'un composant Rigidbody !");
                Destroy(ball);
                return;
            }

            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            // MISE À JOUR IMPORTANTE ICI:
            // Informe l'inventaire que la pokeball n'est plus "en main" (car lancée)
            InventoryManager.Instance.RemovePokeball();
            UpdateHandVisuals(); // Met à jour l'affichage des mains
        }
    }
}