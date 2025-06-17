using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // --- MOUVEMENT (inchangé) ---
    public float moveSpeed = 5.0f;
    public float gravity = -9.81f;
    private CharacterController controller;
    private Vector3 velocity;
    public Camera playerCamera;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    private float rotationX = 0;

    // --- GESTION DES MAINS ET ÉQUIPEMENT (inchangé) ---
    [Header("Hand & Equipment")]
    public Transform rightHandHolder;
    public Transform leftHandHolder;
    public Transform throwPoint;

    [Header("Prefabs Visuels (inchangé)")]
    public GameObject weaponInHandPrefab;
    public GameObject pokeballInHandPrefab;

    [Header("Weapon & Projectiles (inchangé)")]
    public GameObject weaponProjectilePrefab;
    public Transform weaponFirePoint;
    public GameObject pokeballProjectilePrefab;
    public float throwForce = 20f;

    // --- OBJETS & ÉTATS (inchangé) ---
    private GameObject equippedWeapon;
    private GameObject equippedPokeball;
    private bool hasWeapon = false;
    private bool hasPokeballInHand = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
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
        if (hasWeapon && hasPokeballInHand)
        {
            // ÉTAT 3 : DEUX MAINS
            if (Input.GetButtonDown("Fire1")) { ShootWeapon(); }      // Clic Gauche : TIRE avec l'arme (main gauche)
            if (Input.GetButtonDown("Fire2")) { ThrowPokeball(); }    // Clic Droit : LANCE la sphère (main droite)
        }
        else if (hasWeapon)
        {
            // ÉTAT 1 : ARME SEULE (main droite)
            if (Input.GetButtonDown("Fire1")) { ShootWeapon(); }      // Clic Gauche : TIRE avec l'arme
        }
        else if (hasPokeballInHand)
        {
            // ÉTAT 2 : SPHÈRE SEULE (main droite)
            if (Input.GetButtonDown("Fire1")) { ThrowPokeball(); }    // Clic Gauche : LANCE la sphère
        }
    }

    void ThrowPokeball()
    {
        if (pokeballProjectilePrefab == null) return;
        
        GameObject ball = Instantiate(pokeballProjectilePrefab, throwPoint.position, throwPoint.rotation);
        ball.GetComponent<Rigidbody>().AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);

        hasPokeballInHand = false;
        Destroy(equippedPokeball);

        // Si on a encore une arme, elle repasse dans la main droite
        if (equippedWeapon != null)
        {
            equippedWeapon.transform.SetParent(rightHandHolder, false);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
        }
    }

    void ShootWeapon()
    {
        if (weaponProjectilePrefab == null) return;
        Transform firePointToUse = (weaponFirePoint != null) ? weaponFirePoint : throwPoint;
        Instantiate(weaponProjectilePrefab, firePointToUse.position, firePointToUse.rotation);
    }

    // --- MODIFIÉ : Nouvelle logique pour le ramassage ---
    private void OnTriggerEnter(Collider other)
    {
        // Tente de ramasser une arme
        if (other.CompareTag("WeaponPickup") && !hasWeapon)
        {
            // Détruit le pickup au sol
            Destroy(other.gameObject);
            hasWeapon = true;

            // Si on a déjà une sphère en main droite, on la déplace à gauche
            if (hasPokeballInHand)
            {
                equippedPokeball.transform.SetParent(leftHandHolder);
                equippedPokeball.transform.localPosition = Vector3.zero;
                equippedPokeball.transform.localRotation = Quaternion.identity;
            }

            // On crée la nouvelle arme et on la place dans la main droite
            equippedWeapon = Instantiate(weaponInHandPrefab);
            equippedWeapon.transform.SetParent(rightHandHolder);
            equippedWeapon.transform.localPosition = Vector3.zero;
            equippedWeapon.transform.localRotation = Quaternion.identity;
        }
        // Tente de ramasser une sphère
        else if (other.CompareTag("PokeballPickup") && !hasPokeballInHand)
        {
            // Détruit le pickup au sol
            Destroy(other.gameObject);
            hasPokeballInHand = true;

            // Si on a déjà une arme en main droite, on la déplace à gauche
            if (hasWeapon)
            {
                equippedWeapon.transform.SetParent(leftHandHolder);
                equippedWeapon.transform.localPosition = Vector3.zero;
                equippedWeapon.transform.localRotation = Quaternion.identity;
            }

            // On crée la nouvelle sphère et on la place dans la main droite
            equippedPokeball = Instantiate(pokeballInHandPrefab);
            equippedPokeball.transform.SetParent(rightHandHolder);
            equippedPokeball.transform.localPosition = Vector3.zero;
            equippedPokeball.transform.localRotation = Quaternion.identity;
        }
    }
}