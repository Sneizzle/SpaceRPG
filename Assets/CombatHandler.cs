using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    public int maxHealth = 60;           // Max hull health
    public int currentHealth;            // Current hull health
    public int maxShield = 50;           // Max shield capacity
    public int currentShield;            // Current shield health
    public float shieldRegenRate = 0.12f;   // 12% of max shield per second
    public float shieldRegenDelay = 5f;  // Delay before shield starts regenerating
    private float lastDamageTime;        // Time when the last damage was taken
    public GameObject shieldVisual;      // Visual representation of the shield
    public MeshDestroy meshDestroyScript; // Reference to the MeshDestroy script
    public bool isPlayer;                // Check if this is the player's ship

    private float regenTickInterval = 1f; // Interval between shield regen ticks (1 second)
    private float lastRegenTickTime;      // Time of the last shield regeneration tick

    void Start()
    {
        currentHealth = maxHealth;       // Set hull health to max at the start
        currentShield = maxShield;       // Set shield to max at the start
        lastDamageTime = Time.time;      // Initialize last damage time
        lastRegenTickTime = Time.time;   // Initialize last regen tick time
    }

    void Update()
    {
        // Check if enough time has passed for shield regeneration (5 seconds after last damage)
        if (currentShield < maxShield && Time.time >= lastDamageTime + shieldRegenDelay)
        {
            if (Time.time >= lastRegenTickTime + regenTickInterval)
            {
                RegenerateShield();
                lastRegenTickTime = Time.time; // Reset tick timer
            }
        }

        // Hide or show the shield visual based on shield health
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(currentShield > 0); // Hide shield if currentShield is 0
        }
    }

    // Method to apply damage
    public void TakeDamage(int damage)
    {
        if (currentShield > 0)
        {
            // Apply damage to the shield first
            currentShield -= damage;
            if (currentShield < 0)
            {
                // If damage exceeds shield capacity, apply remaining damage to hull
                currentHealth += currentShield; // currentShield is negative here
                currentShield = 0;
            }
        }
        else
        {
            // If the shield is depleted, apply damage to hull
            currentHealth -= damage;
        }

        Debug.Log(gameObject.name + " took " + damage + " damage, remaining health: " + currentHealth + ", shield: " + currentShield);

        // Check if health has fallen below zero
        if (currentHealth <= 0)
        {
            Die();                       // Call death method if health is 0 or less
        }

        // Reset shield regeneration timer whenever damage is taken
        lastDamageTime = Time.time;
    }

    // Method to regenerate shield
    void RegenerateShield()
    {
        // Regenerate 12% of max shield per tick (every second)
        int shieldRegenAmount = Mathf.RoundToInt(maxShield * shieldRegenRate);
        currentShield += shieldRegenAmount;

        // Ensure shield does not exceed the max shield
        currentShield = Mathf.Min(currentShield, maxShield);
        Debug.Log("Regenerating shield: " + shieldRegenAmount + ", current shield: " + currentShield);
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has been destroyed!");
        meshDestroyScript.DestroyMesh();  // Call the MeshDestroy method
        Destroy(gameObject);
    }
}
