using UnityEngine;
using UnityEngine.UI; // Important pour utiliser le Slider

public class PokemonController : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public Slider healthSlider; // Référence à notre barre de vie
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if(healthSlider != null) 
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if(healthSlider != null) 
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