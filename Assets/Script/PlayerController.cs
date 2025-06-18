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
        firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f); // Ajuste ces valeurs pour qu'il soit bien visible
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

    // --- LOGIQUE D'ACTIONS (MISE À JOUR) ---
    void HandleInput()
    {
        // Vérifie si le temps de rechargement de l'arme est passé
        if (Time.time < nextFireTime) return;

        // Priorité : Si l'arme est dans la main gauche (quand les deux sont équipées)
        if (currentLeftHandItemVisual != null && currentLeftHandItemVisual.CompareTag("Weapon"))
        {
            if (Input.GetButton("Fire1")) // Clic Gauche : TIRE
            {
                ShootWeapon();
            }
        }
        // Sinon, si l'arme est dans la main droite (quand l'arme seule est équipée)
        else if (currentRightHandItemVisual != null && currentRightHandItemVisual.CompareTag("Weapon"))
        {
            if (Input.GetButton("Fire1")) // Clic Gauche : TIRE
            {
                ShootWeapon();
            }
        }

        // Si la Pokeball est dans la main droite (quand les deux sont équipées)
        if (currentRightHandItemVisual != null && currentRightHandItemVisual.CompareTag("Pokeball"))
        {
            if (Input.GetButtonDown("Fire2")) // Clic Droit : LANCE
            {
                ThrowPokeball();
            }
        }
        // Sinon, si seule la Pokeball est équipée (elle est donc dans la main droite)
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
            Destroy(other.gameObject); // Détruit l'objet de ramassage
            hasWeaponEquipped = true;
            UpdateHandVisuals(); // Met à jour la représentation visuelle
        }
        else if (other.CompareTag("PokeballPickup") && !hasPokeballEquipped)
        {
            Destroy(other.gameObject); // Détruit l'objet de ramassage
            hasPokeballEquipped = true;
            UpdateHandVisuals(); // Met à jour la représentation visuelle
        }
    }

    // --- FONCTION CENTRALE "CHEF D'ORCHESTRE" ---
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
        if (hasWeaponEquipped && hasPokeballEquipped) // Cas : Les deux objets sont tenus
        {
            // L'arme va à la main gauche
            currentLeftHandItemVisual = Instantiate(weaponVisualPrefab, leftHandHolder);
            currentLeftHandItemVisual.tag = "Weapon"; // Assure que le visuel a aussi un tag
            // Ajustez ces valeurs pour le positionnement de l'arme dans la main gauche
            currentLeftHandItemVisual.transform.localPosition = Vector3.zero;
            currentLeftHandItemVisual.transform.localRotation = Quaternion.identity;

            // La Pokeball va à la main droite
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball"; // Assure que le visuel a aussi un tag
            // Ajustez ces valeurs pour le positionnement de la pokeball dans la main droite
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Le point de tir suit l'arme (main gauche)
            firePoint.SetParent(leftHandHolder);
            firePoint.localPosition = Vector3.zero; // Ajustez par rapport à l'arme
            firePoint.localRotation = Quaternion.identity;
        }
        else if (hasWeaponEquipped && !hasPokeballEquipped) // Cas : Seule l'arme est tenue
        {
            // L'arme va à la main droite (car c'est le seul objet)
            currentRightHandItemVisual = Instantiate(weaponVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Weapon";
            // Ajustez ces valeurs pour le positionnement de l'arme dans la main droite
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Le point de tir suit l'arme (main droite)
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Ajustez par rapport à l'arme
            firePoint.localRotation = Quaternion.identity;
        }
        else if (!hasWeaponEquipped && hasPokeballEquipped) // Cas : Seule la Pokeball est tenue
        {
            // La Pokeball va à la main droite (car c'est le seul objet)
            currentRightHandItemVisual = Instantiate(pokeballVisualPrefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            // Ajustez ces valeurs pour le positionnement de la pokeball dans la main droite
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Le point de tir suit la pokeball (main droite)
            firePoint.SetParent(rightHandHolder);
            firePoint.localPosition = Vector3.zero; // Ajustez par rapport à la pokeball
            firePoint.localRotation = Quaternion.identity;
        }
        else // Aucun objet équipé
        {
            // Assure que le point de tir est dans une position neutre
            firePoint.SetParent(playerCamera.transform);
            firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f); // Position initiale par rapport à la caméra
            firePoint.localRotation = Quaternion.identity;
        }

        // Astuce : Ajustez localPosition et localRotation pour chaque prefab visuel dans l'éditeur
        // après l'avoir instancié dans une scène temporaire pour trouver les bonnes valeurs,
        // puis copiez-les ici.
    }

    // --- Fonctions d'action ---
    void ShootWeapon()
    {
        // S'assure qu'une arme est bien équipée
        if (hasWeaponEquipped)
        {
            // Instancie le projectile à la position et rotation du firePoint
            Instantiate(weaponProjectilePrefab, firePoint.position, firePoint.rotation);
            nextFireTime = Time.time + fireRate; // Applique le délai de tir
        }
    }

    void ThrowPokeball()
    {
        // S'assure qu'une pokeball est bien équipée
        if (hasPokeballEquipped)
        {
            // Instancie la pokeball à la position et rotation du firePoint
            GameObject ball = Instantiate(pokeballProjectilePrefab, firePoint.position, firePoint.rotation);

            // Assure que le projectile a un Rigidbody pour appliquer une force
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Le préfabriqué de la Pokeball Projectile a besoin d'un composant Rigidbody !");
                Destroy(ball); // Détruit l'objet s'il ne peut pas être lancé
                return;
            }

            // Applique une force pour lancer la pokeball dans la direction de la caméra
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.Impulse);

            // Une fois lancée, la pokeball n'est plus "en main"
            hasPokeballEquipped = false;
            UpdateHandVisuals(); // Met à jour l'affichage des mains
        }
    }
}