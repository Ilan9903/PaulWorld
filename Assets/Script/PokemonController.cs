using UnityEngine;
using UnityEngine.AI; // MODIFIÉ : Indispensable pour utiliser le NavMeshAgent
using UnityEngine.UI; 

[RequireComponent(typeof(NavMeshAgent))] // MODIFIÉ : S'assure que le composant est bien là
public class PokemonController : MonoBehaviour // CONSERVÉ : Ton nom de classe
{
    // --- NOUVEAU : Système de Mouvement avec NavMeshAgent ---
    [Header("Movement AI")]
    public float wanderRadius = 20f; // Rayon de la zone où il peut se balader
    public float wanderTimer = 5f;   // Temps de marche avant une pause
    public float idleTimer = 3f;     // Temps de pause

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float timer;
    private bool isWandering = true;

    // --- CONSERVÉ : Logique de Vie ---
    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthSlider; // Référence à notre barre de vie
    private float currentHealth;

    void Start()
    {
        // MODIFIÉ : Initialisation du NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position; // Mémorise son point de départ
        timer = wanderTimer;

        // CONSERVÉ : Initialisation de la vie
        currentHealth = maxHealth;
        if(healthSlider != null) 
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    
    void Update()
    {
        // MODIFIÉ : Toute la logique de déplacement est remplacée par celle-ci
        timer += Time.deltaTime;

        healthSlider.transform.LookAt(Camera.main.transform);

        if (isWandering)
        {
            // Si le temps de marche est écoulé, on fait une pause
            if (timer >= wanderTimer)
            {
                agent.isStopped = true; // Arrête le mouvement
                isWandering = false;
                timer = 0;
            }
        }
        else
        {
            // Si le temps de pause est écoulé, on trouve une nouvelle destination
            if (timer >= idleTimer)
            {
                Vector3 newPos = RandomNavSphere(startPosition, wanderRadius, -1);
                agent.SetDestination(newPos);
                agent.isStopped = false; // Reprend le mouvement
                isWandering = true;
                timer = 0;
            }
        }
    }

    // NOUVEAU : Fonction helper pour trouver un point sur le NavMesh
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    // MODIFIÉ : Condition de capture mise à jour
    public void AttemptToCapture()
    {
        // Condition : la vie doit être inférieure ou égale à 30
        if (currentHealth <= 30)
        {
            Debug.Log("Capture Réussie !");
            // La ligne ci-dessous sera pour plus tard, quand PlayerStats existera
            // FindObjectOfType<PlayerStats>().AddXp(50); 
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Capture Échouée ! La créature est encore trop forte (" + currentHealth + " PV).");
        }
    }

    // CONSERVÉ : Ta logique de dégâts est parfaite
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        Debug.Log(gameObject.name + " a maintenant " + currentHealth + " PV.");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // CONSERVÉ : Ta logique de mort est parfaite
    void Die()
    {
        Debug.Log(gameObject.name + " est vaincu.");
        Destroy(gameObject);
    }
}