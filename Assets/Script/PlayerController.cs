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
    // Le firePoint est maintenant un enfant permanent de la caméra pour éviter les MissingReferenceException
    public Transform firePoint; 

    [Header("Item Prefabs")]
    public GameObject WeaponInHand_Prefab;       // Le modèle 3D de l'arme
    public GameObject PokeballInHand_Prefab;     // Le modèle 3D de la sphère
    public GameObject Projectile;   // La "balle" tirée par l'arme
    public GameObject pokeballProjectilePrefab; // La sphère à lancer

    [Header("Item Stats")]
    public float throwForce = 15f;
    public float fireRate = 0.5f; // 2 tirs par seconde
    private float nextFireTime = 0f;

    // --- GESTION D'ÉTAT ---
    private GameObject currentRightHandItemVisual = null; // Stocke l'objet visuel actuel dans la main droite
    private GameObject currentLeftHandItemVisual = null;  // Stocke l'objet visuel actuel dans la main gauche

    private bool hasWeaponEquipped = false;
    private bool hasPokeballEquipped = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Assure que le firePoint est un enfant direct et permanent de la caméra du joueur
        // Sa position sera ajustée dynamiquement dans UpdateHandVisuals
        firePoint.SetParent(playerCamera.transform);
        // Définir une position locale par défaut pour le firePoint
        // Ces valeurs peuvent être ajustées directement dans l'éditeur pendant le jeu pour un meilleur aperçu
        firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f); 
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
        // Logique de l'arme (Clic Gauche)
        // Vérifie si une arme est équipée et si le délai de tir est passé
        if (hasWeaponEquipped && Time.time >= nextFireTime)
        {
            // Input.GetButton pour un tir continu si le bouton est maintenu
            if (Input.GetButton("Fire1")) // Clic Gauche
            {
                ShootWeapon();
            }
        }

        // Logique de la Pokeball (Clic Droit)
        // Vérifie si une pokeball est équipée
        if (hasPokeballEquipped)
        {
            // Input.GetButtonDown pour un lancer unique par clic
            if (Input.GetButtonDown("Fire2")) // Clic Droit
            {
                ThrowPokeball();
            }
            // Cas spécial : Si SEULEMENT la pokeball est équipée, utiliser le Clic Gauche pour lancer
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

    // --- FONCTION CENTRALE "CHEF D'ORCHESTRE" (MODIFIÉE) ---
    void UpdateHandVisuals()
    {
        // 1. Dégager les mains actuelles (détruire les anciens objets visuels)
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

        // 2. Décider quoi équiper et où, et ajuster la position du firePoint
        if (hasWeaponEquipped && hasPokeballEquipped) // Cas : Les deux objets sont tenus
        {
            // L'arme va à la main gauche
            currentLeftHandItemVisual = Instantiate(WeaponInHand_Prefab, leftHandHolder);
            currentLeftHandItemVisual.tag = "Weapon"; // Assure que le visuel a le bon tag
            // Ajustez ces valeurs pour le positionnement de l'arme dans la main gauche (local à leftHandHolder)
            currentLeftHandItemVisual.transform.localPosition = Vector3.zero;
            currentLeftHandItemVisual.transform.localRotation = Quaternion.identity;

            // La Pokeball va à la main droite
            currentRightHandItemVisual = Instantiate(PokeballInHand_Prefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball"; // Assure que le visuel a le bon tag
            // Ajustez ces valeurs pour le positionnement de la pokeball dans la main droite (local à rightHandHolder)
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Positionne le firePoint pour l'arme en main gauche
            // Notez: firePoint.localPosition est maintenant relatif à playerCamera.transform
            // Les valeurs ci-dessous sont des exemples, à ajuster précisément.
            firePoint.localPosition = leftHandHolder.localPosition + new Vector3(0.2f, 0, 0.5f); 
            firePoint.localRotation = leftHandHolder.localRotation;
        }
        else if (hasWeaponEquipped && !hasPokeballEquipped) // Cas : Seule l'arme est tenue
        {
            // L'arme va à la main droite (car c'est le seul objet)
            currentRightHandItemVisual = Instantiate(WeaponInHand_Prefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Weapon";
            // Ajustez ces valeurs pour le positionnement de l'arme dans la main droite (local à rightHandHolder)
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Positionne le firePoint pour l'arme en main droite
            // Notez: firePoint.localPosition est maintenant relatif à playerCamera.transform
            firePoint.localPosition = rightHandHolder.localPosition + new Vector3(-0.2f, 0, 0.5f); 
            firePoint.localRotation = rightHandHolder.localRotation;
        }
        else if (!hasWeaponEquipped && hasPokeballEquipped) // Cas : Seule la Pokeball est tenue
        {
            // La Pokeball va à la main droite (car c'est le seul objet)
            currentRightHandItemVisual = Instantiate(PokeballInHand_Prefab, rightHandHolder);
            currentRightHandItemVisual.tag = "Pokeball";
            // Ajustez ces valeurs pour le positionnement de la pokeball dans la main droite (local à rightHandHolder)
            currentRightHandItemVisual.transform.localPosition = Vector3.zero;
            currentRightHandItemVisual.transform.localRotation = Quaternion.identity;

            // Positionne le firePoint pour la pokeball en main droite
            // Notez: firePoint.localPosition est maintenant relatif à playerCamera.transform
            firePoint.localPosition = rightHandHolder.localPosition + new Vector3(0, 0, 0.3f); 
            firePoint.localRotation = rightHandHolder.localRotation;
        }
        else // Aucun objet équipé
        {
            // Réinitialise le firePoint à sa position par défaut par rapport à la caméra
            firePoint.localPosition = new Vector3(0.5f, -0.2f, 1f);
            firePoint.localRotation = Quaternion.identity;
        }

        // CONSEIL IMPORTANT: Les valeurs de localPosition et localRotation (Vector3.zero, Quaternion.identity)
        // sont des points de départ. Pour que vos objets visuels (arme, pokeball) s'affichent correctement
        // dans les mains, et pour que le firePoint soit à la bonne place (bouche de l'arme/centre de la pokeball),
        // vous devrez les ajuster finement dans l'éditeur Unity. Lancez le jeu, sélectionnez votre joueur,
        // trouvez l'objet visuel instancié dans la main (sous rightHandHolder/leftHandHolder) ou le firePoint
        // (sous playerCamera), déplacez-les/faites-les pivoter pour qu'ils soient parfaits, puis copiez
        // les valeurs Local Position et Local Rotation affichées dans l'Inspector dans votre code.
    }

    // --- Fonctions d'action ---
    void ShootWeapon()
    {
        // S'assure qu'une arme est bien équipée
        if (hasWeaponEquipped)
        {
            // Instancie le projectile à la position et rotation du firePoint
            Instantiate(Projectile, firePoint.position, firePoint.rotation);
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