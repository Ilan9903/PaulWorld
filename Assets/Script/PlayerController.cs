using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT (inchangé) ---
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 2.0f;
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
    // Supprimez throwPoint ici si weaponFirePoint est utilisé pour les tirs de pokeball aussi
    // Si la Pokeball a un point de lancement spécifique sur son modèle en main, utilisez-le.
    // Sinon, on peut réutiliser weaponFirePoint ou créer un pokeballFirePoint sur le modèle de pokeball.
    // Pour l'instant, on va considérer que throwPoint est le point de départ des projectiles,
    // et il sera attaché à la main qui tient l'objet qui lance.
    public Transform throwPoint; // Le point d'où les projectiles (balle ou pokeball) partent.


    [Header("Prefabs Visuels")]
    public GameObject weaponInHandPrefab;   // Le prefab du MODÈLE D'ARME que le joueur tient
    public GameObject pokeballInHandPrefab; // Le prefab du MODÈLE DE POKEBALL que le joueur tient

    [Header("Weapon & Projectiles")]
    public GameObject weaponProjectilePrefab; // Le prefab du PROJECTILE que l'arme tire
    // weaponFirePoint devrait être un enfant du weaponInHandPrefab
    // On va le chercher dynamiquement sur l'objet équipé.
    // public Transform weaponFirePoint; // <-- Retire cette ligne, on la trouvera sur equippedWeapon
    public GameObject pokeballProjectilePrefab; // Le prefab du PROJECTILE de la pokeball
    public float throwForce = 20f;

    // --- OBJETS & ÉTATS ---
    private GameObject equippedWeapon;      // L'instance actuelle du MODÈLE D'ARME dans la scène
    private GameObject equippedPokeball;    // L'instance actuelle du MODÈLE DE POKEBALL dans la scène
    private bool hasWeapon = false;
    private bool hasPokeballInHand = false; // Renommée pour éviter confusion avec le projectile

    // --- NOUVEAU : Référence pour le prochain tir / lancer ---
    private float nextActionTime = 0f;
    public float weaponFireRate = 0.5f; // Taux de tir de l'arme
    // public float pokeballThrowRate = 1.0f; // Si vous voulez un délai entre les lancers de pokeballs

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Assurez-vous que les objets équipés sont nulls au départ.
        // Ils seront instanciés quand ramassés.
        equippedWeapon = null;
        equippedPokeball = null;

        // Vérification des points de main pour éviter les erreurs
        if (rightHandHolder == null) Debug.LogError("rightHandHolder n'est pas assigné dans PlayerController.");
        if (leftHandHolder == null) Debug.LogError("leftHandHolder n'est pas assigné dans PlayerController.");
        if (throwPoint == null) Debug.LogError("throwPoint n'est pas assigné dans PlayerController. Ce point doit être un enfant du joueur pour le départ des projectiles.");

        // Assurez-vous que les prefabs visuels sont assignés
        if (weaponInHandPrefab == null) Debug.LogWarning("weaponInHandPrefab n'est pas assigné. L'arme visuelle ne sera pas affichée.");
        if (pokeballInHandPrefab == null) Debug.LogWarning("pokeballInHandPrefab n'est pas assigné. La pokeball visuelle ne sera pas affichée.");
    }

    void Update()
    {
        HandleMovement();
        HandleInput(); // Cette fonction est maintenant beaucoup plus intelligente
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

        // Saut
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            // Déclenche le saut si la barre espace est pressée
            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        float lookY = Input.GetAxis("Mouse X") * lookSpeed;
        rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, lookY, 0);
    }

    // --- MODIFIÉ : Nouvelle logique pour les actions ---
    void HandleInput()
    {
        // Rappel: Fire1 = Clic Gauche, Fire2 = Clic Droit

        // Gère le délai de tir/lancement
        if (Time.time < nextActionTime) return; // Si le délai n'est pas écoulé, on ne fait rien.

        if (hasWeapon && hasPokeballInHand)
        {
            // ÉTAT 3 : DEUX MAINS
            if (Input.GetButtonDown("Fire1")) // Clic Gauche : TIRE avec l'arme (main gauche)
            {
                ShootWeapon();
                nextActionTime = Time.time + weaponFireRate; // Définit le prochain temps de tir
            }
            else if (Input.GetButtonDown("Fire2")) // Clic Droit : LANCE la sphère (main droite)
            {
                ThrowPokeball();
                // nextActionTime = Time.time + pokeballThrowRate; // Activez si vous voulez un délai pour la pokeball
            }
        }
        else if (hasWeapon)
        {
            // ÉTAT 1 : ARME SEULE (main droite)
            if (Input.GetButtonDown("Fire1")) // Clic Gauche : TIRE avec l'arme
            {
                ShootWeapon();
                nextActionTime = Time.time + weaponFireRate;
            }
        }
        else if (hasPokeballInHand)
        {
            // ÉTAT 2 : SPHÈRE SEULE (main droite)
            if (Input.GetButtonDown("Fire1")) // Clic Gauche : LANCE la sphère
            {
                ThrowPokeball();
                // nextActionTime = Time.time + pokeballThrowRate;
            }
        }
    }

    void ThrowPokeball()
    {
        if (pokeballProjectilePrefab == null)
        {
            Debug.LogWarning("pokeballProjectilePrefab n'est pas assigné.");
            return;
        }
        if (throwPoint == null)
        {
            Debug.LogError("throwPoint n'est pas assigné ! Impossible de lancer la pokeball.");
            return;
        }

        // Instancie le projectile Pokeball à l'emplacement du throwPoint
        GameObject ball = Instantiate(pokeballProjectilePrefab, throwPoint.position, throwPoint.rotation);
        
        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        if (ballRb != null)
        {
            ballRb.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("La pokeball projectile n'a pas de Rigidbody.");
        }

        // Détruit l'objet visuel de la pokeball dans la main du joueur
        if (equippedPokeball != null)
        {
            Destroy(equippedPokeball);
            equippedPokeball = null; // Assurez-vous de nullifier la référence
        }
        hasPokeballInHand = false; // Met à jour l'état de possession

        // Appelle la fonction de mise à jour des mains après le lancer
        UpdateHandEquipment();
    }

    void ShootWeapon()
    {
        if (weaponProjectilePrefab == null)
        {
            Debug.LogWarning("weaponProjectilePrefab n'est pas assigné.");
            return;
        }
        if (throwPoint == null) // Utilisez throwPoint comme point de tir général
        {
            Debug.LogError("throwPoint n'est pas assigné ! Impossible de tirer.");
            return;
        }

        // Instancie le projectile de l'arme
        Instantiate(weaponProjectilePrefab, throwPoint.position, throwPoint.rotation);
        // Si votre arme équipée a un point de tir spécifique (par exemple, un enfant de equippedWeapon),
        // vous pouvez faire :
        // Transform currentWeaponFirePoint = equippedWeapon.transform.Find("WeaponFirePoint");
        // Instantiate(weaponProjectilePrefab, currentWeaponFirePoint.position, currentWeaponFirePoint.rotation);
    }

    // --- MODIFIÉ : Nouvelle logique pour le ramassage et la gestion visuelle des mains ---
    private void OnTriggerEnter(Collider other)
    {
        bool consumedPickup = false;

        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            hasWeapon = true;
            consumedPickup = true;

            // Détruit l'objet pickup au sol
            Destroy(other.gameObject);

            // Instancie l'arme visuelle si le prefab est défini
            if (weaponInHandPrefab != null)
            {
                equippedWeapon = Instantiate(weaponInHandPrefab);
                // Ne pas le positionner tout de suite, UpdateHandEquipment le fera
            }
            UpdateHandEquipment(); // Met à jour l'affichage après avoir ramassé l'arme
        }
        else if (other.CompareTag("PokeballPickup") && !hasPokeballInHand)
        {
            hasPokeballInHand = true;
            consumedPickup = true;

            // Détruit l'objet pickup au sol
            Destroy(other.gameObject);

            // Instancie la pokeball visuelle si le prefab est défini
            if (pokeballInHandPrefab != null)
            {
                equippedPokeball = Instantiate(pokeballInHandPrefab);
                // Ne pas le positionner tout de suite, UpdateHandEquipment le fera
            }
            UpdateHandEquipment(); // Met à jour l'affichage après avoir ramassé la pokeball
        }
        // Pas besoin de Destroy(other.gameObject) ici, car c'est fait directement dans les blocs if
        // Cela garantit que le pickup est détruit UNIQUEMENT s'il est ramassé.
    }

    // --- NOUVELLE FONCTION : Gère la position des objets dans les mains ---
    void UpdateHandEquipment()
    {
        // Désactiver tous les objets équipés pour les réinitialiser
        if (equippedWeapon != null) equippedWeapon.SetActive(false);
        if (equippedPokeball != null) equippedPokeball.SetActive(false);

        // Réinitialiser les parents pour éviter les problèmes si on change de main
        if (equippedWeapon != null) equippedWeapon.transform.SetParent(null);
        if (equippedPokeball != null) equippedPokeball.transform.SetParent(null);

        if (hasWeapon && hasPokeballInHand)
        {
            // Les deux objets : Arme à gauche, Pokeball à droite
            if (equippedWeapon != null)
            {
                equippedWeapon.SetActive(true);
                equippedWeapon.transform.SetParent(leftHandHolder);
                equippedWeapon.transform.localPosition = Vector3.zero;
                equippedWeapon.transform.localRotation = Quaternion.identity;
                // Si l'arme a besoin d'une rotation/position spécifique par rapport au holder, ajustez ici.
                // Ex: equippedWeapon.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            if (equippedPokeball != null)
            {
                equippedPokeball.SetActive(true);
                equippedPokeball.transform.SetParent(rightHandHolder);
                equippedPokeball.transform.localPosition = Vector3.zero;
                equippedPokeball.transform.localRotation = Quaternion.identity;
            }
            // Positionne le throwPoint à droite car c'est la main qui lance la Pokeball
            if (throwPoint != null)
            {
                throwPoint.SetParent(rightHandHolder);
                throwPoint.localPosition = Vector3.zero;
                throwPoint.localRotation = Quaternion.identity;
            }
        }
        else if (hasWeapon && !hasPokeballInHand)
        {
            // Seulement l'arme : à droite
            if (equippedWeapon != null)
            {
                equippedWeapon.SetActive(true);
                equippedWeapon.transform.SetParent(rightHandHolder);
                equippedWeapon.transform.localPosition = Vector3.zero;
                equippedWeapon.transform.localRotation = Quaternion.identity;
            }
            // Positionne le throwPoint à droite car c'est la main qui tire avec l'arme seule
            if (throwPoint != null)
            {
                throwPoint.SetParent(rightHandHolder);
                throwPoint.localPosition = Vector3.zero;
                throwPoint.localRotation = Quaternion.identity;
            }
        }
        else if (!hasWeapon && hasPokeballInHand)
        {
            // Seulement la Pokeball : à droite
            if (equippedPokeball != null)
            {
                equippedPokeball.SetActive(true);
                equippedPokeball.transform.SetParent(rightHandHolder);
                equippedPokeball.transform.localPosition = Vector3.zero;
                equippedPokeball.transform.localRotation = Quaternion.identity;
            }
            // Positionne le throwPoint à droite car c'est la main qui lance la Pokeball seule
            if (throwPoint != null)
            {
                throwPoint.SetParent(rightHandHolder);
                throwPoint.localPosition = Vector3.zero;
                throwPoint.localRotation = Quaternion.identity;
            }
        }
        else
        {
            // Rien en main, s'assurer que throwPoint n'est attaché à aucune main ou à la racine du joueur
            if (throwPoint != null)
            {
                throwPoint.SetParent(this.transform); // Attache throwPoint au joueur lui-même
                // Ajustez la position locale si nécessaire, ou gardez-le à la main droite par défaut
                throwPoint.localPosition = new Vector3(0.5f, 1.5f, 0.5f); // Exemple de position par défaut
            }
        }
    }
}