using UnityEngine;
using UnityEngine.UI; // Important pour utiliser le Slider

public class PokemonController : MonoBehaviour
{

    public Vector2 zoneMin; // Coin inférieur gauche de la zone
    public Vector2 zoneMax; // Coin supérieur droit de la zone
    public float speed = 2f;
    private Vector3 targetPosition;

    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthSlider; // Référence à notre barre de vie
    private float currentHealth;

    void Start()
    {
        SetNewRandomTarget();

        currentHealth = maxHealth;
        if(healthSlider != null) 
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
    
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewRandomTarget();
        }
    }

    void SetNewRandomTarget()
    {
        float x = Random.Range(zoneMin.x, zoneMax.x);
        float y = Random.Range(zoneMin.y, zoneMax.y);
        targetPosition = new Vector3(x, y, transform.position.z);
    }

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

    void Die()
    {
        Debug.Log(gameObject.name + " est vaincu.");
        Destroy(gameObject);
    }
}