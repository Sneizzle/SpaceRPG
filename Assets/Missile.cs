using UnityEngine;
using UnityEngine.UI; // Import UI namespace

public class Missile : MonoBehaviour
{
    public GameObject missilePrefab;           // Assign the missile prefab in Inspector
    public Transform missileSpawnPoint;        // Where the missile will spawn (e.g., the ship's wings)
    public float missileSpeed = 50f;           // Speed of the missile
    public float explosionDistance = 100f;     // Public property to control explosion distance
    public GameObject explosionEffect;         // Explosion effect prefab
    public Collider spaceshipCollider;         // Assign the spaceship's collider in the Inspector
    public float missileCooldown = 2f;         // Time between missile reloads
    public int maxMissiles = 4;                // Maximum missiles that can be saved up

    public Slider cooldownSlider;              // Reference to the UI cooldown slider
    public Text missileCountText;              // Reference to the UI text showing missile count

    private Camera mainCamera;
    private float lastFiredTime;               // Keeps track of the last time a missile was fired
    private int currentMissileCount;           // Tracks how many missiles are ready to fire
    private float missileFireDelay = 0.2f;     // Delay between consecutive shots when holding down

    void Start()
    {
        mainCamera = Camera.main;              // Get the main camera for aiming
        lastFiredTime = -missileCooldown;      // Initialize so the player can shoot immediately
        currentMissileCount = maxMissiles;     // Start with full missiles
        cooldownSlider.maxValue = missileCooldown; // Set slider max value to cooldown duration
        cooldownSlider.value = missileCooldown;    // Start with a full bar
        UpdateMissileCountUI();
    }

    void Update()
    {
        // Update the cooldown slider
        float timeSinceLastFire = Time.time - lastFiredTime;
        cooldownSlider.value = missileCooldown - Mathf.Clamp(timeSinceLastFire, 0, missileCooldown);

        // Reload missiles if needed and cooldown has passed
        if (currentMissileCount < maxMissiles && Time.time >= lastFiredTime + missileCooldown)
        {
            currentMissileCount++;
            lastFiredTime = Time.time;  // Reset the cooldown timer
            UpdateMissileCountUI();
        }

        // Check if we can shoot and hold down right-click
        if (Input.GetMouseButton(1) && currentMissileCount > 0 && Time.time >= lastFiredTime + missileFireDelay)
        {
            ShootMissile();
            lastFiredTime = Time.time; // Update the last fired time
        }
    }

    void ShootMissile()
    {
        // Instantiate the missile at the spawn point
        GameObject missile = Instantiate(missilePrefab, missileSpawnPoint.position, missileSpawnPoint.rotation);

        // Get the direction the camera is facing (for aiming)
        Vector3 shootDirection = mainCamera.transform.forward;

        // Set the missile's velocity in the direction the player is aiming
        Rigidbody missileRb = missile.GetComponent<Rigidbody>();
        missileRb.velocity = shootDirection * missileSpeed;

        // Pass the explosion distance and effect to the missile behavior script
        MissileBehavior missileBehavior = missile.GetComponent<MissileBehavior>();
        missileBehavior.InitializeMissile(explosionDistance, explosionEffect);

        // Ignore collision between the missile and the spaceship (not the player)
        if (spaceshipCollider != null)
        {
            Physics.IgnoreCollision(missile.GetComponent<Collider>(), spaceshipCollider);
        }

        // Reduce missile count
        currentMissileCount--;
        UpdateMissileCountUI();
    }

    void UpdateMissileCountUI()
    {
        missileCountText.text = currentMissileCount.ToString();  // Display missile count in UI
    }
}
