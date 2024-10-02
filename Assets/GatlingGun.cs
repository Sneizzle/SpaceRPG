using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Import UI namespace

public class GatlingGun : MonoBehaviour
{
    public GameObject bulletPrefab;         // The bullet prefab (small projectile)
    public Transform bulletSpawnPoint;      // Where the bullets will spawn
    public float bulletSpeed = 100f;        // Speed of the bullet
    public float fireRate = 0.2f;           // Time between shots (0.2 seconds between each shot)
    public int maxAmmo = 50;                // Max ammo in one magazine
    public float reloadTime = 2f;           // Time taken to reload

    public Slider ammoSlider;               // UI to display the ammo count
    public Text ammoCountText;              // UI to show the ammo count as a number

    private Camera mainCamera;
    private int currentAmmo;                // Tracks current ammo in the magazine
    private float lastFiredTime;            // Time since the last shot was fired
    private bool isReloading = false;       // Tracks if the gun is reloading

    void Start()
    {
        mainCamera = Camera.main;              // Get the main camera for aiming
        currentAmmo = maxAmmo;                 // Start with a full magazine
        ammoSlider.maxValue = maxAmmo;         // Set slider max value to max ammo
        ammoSlider.value = currentAmmo;        // Initialize with full ammo
        UpdateAmmoCountUI();
    }

    void Update()
    {
        // If reloading, don't shoot
        if (isReloading) return;

        // Check if left-click is held down to shoot
        if (Input.GetMouseButton(0) && Time.time >= lastFiredTime + fireRate && currentAmmo > 0)
        {
            ShootBullet();
            lastFiredTime = Time.time;  // Update the last fired time
        }

        // If out of ammo, start reloading
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    void ShootBullet()
    {
        // Instantiate the bullet at the spawn point
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);

        // Get the direction the camera is facing for aiming
        Vector3 shootDirection = mainCamera.transform.forward;

        // Set the bullet's velocity
        Rigidbody bulletRb = bullet.GetComponent<Rigidbody>();
        bulletRb.velocity = shootDirection * bulletSpeed;

        // Reduce the current ammo count
        currentAmmo--;
        UpdateAmmoCountUI();
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        yield return new WaitForSeconds(reloadTime);  // Wait for reload time

        currentAmmo = maxAmmo;  // Refill the ammo
        UpdateAmmoCountUI();
        isReloading = false;
        Debug.Log("Reload complete.");
    }

    void UpdateAmmoCountUI()
    {
        ammoCountText.text = currentAmmo.ToString();  // Update ammo count in the UI
        ammoSlider.value = currentAmmo;               // Update ammo slider
    }
}
