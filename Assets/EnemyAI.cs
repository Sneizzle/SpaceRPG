using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;             // Reference to the player's position
    public float speed = 50f;            // Speed of the enemy ship
    public float rotationSpeed = 1.5f;   // How fast the enemy ship rotates
    public float engagementDistance = 1000f;  // Distance within which the enemy engages the player
    public float shootDistance = 500f;   // Distance to start shooting at the player
    public float breakOffDistance = 300f;    // Distance to break off and circle
    public float detectionRange = 1000f;     // Range within which the enemy detects the player

    public GameObject missilePrefab;     // Missile prefab for shooting
    public Transform missileSpawnPoint;  // Where missiles are spawned from
    public float missileSpeed = 100f;    // Speed of the missile
    public GameObject explosionEffect;   // Explosion effect prefab

    public Transform rightGun;           // Reference to the RightGUN child

    public int maxShots = 3;             // Number of shots before reload
    public float reloadTime = 5f;        // Time to reload after maxShots
    public float minShootInterval = 1f;  // Minimum time between shots
    public float maxShootInterval = 2.5f;// Maximum time between shots

    public float shootingAngle = 60f;    // Angle within which the enemy can shoot
    public bool debugRaycast = true;     // Toggle for raycast debug

    private float lastShotTime = 0f;
    private int shotsFired = 0;
    private bool reloading = false;
    private float reloadStartTime = 0f;
    private bool playerDetected = false;

    private bool circling = false;       // Determines if the enemy is in circling mode

    void Update()
    {
        // Distance between the enemy and the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Detect the player within detectionRange
        if (distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
        }

        if (playerDetected)
        {
            HandleMovement(distanceToPlayer);

            if (!reloading)
            {
                HandleShooting(distanceToPlayer);
            }
            else if (Time.time > reloadStartTime + reloadTime)
            {
                reloading = false; // Reload is complete
                shotsFired = 0;    // Reset shots

                // Break out of circling for an attack run after reloading
                BreakOutAndAttack();
            }
        }
    }

    void HandleMovement(float distanceToPlayer)
    {
        // Idle until player is within engagement distance
        if (distanceToPlayer > engagementDistance)
        {
            return; // Do nothing while idle
        }

        // Always move forward
        transform.position += transform.forward * speed * Time.deltaTime;

        // Avoid ramming by staying a minimum distance away
        if (distanceToPlayer <= breakOffDistance)
        {
            circling = true;
        }
        else
        {
            circling = false;
        }

        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        if (circling)
        {
            // Circling movement with smoother turns and ship roll
            Vector3 relativePosition = player.position - transform.position;

            // Find random positions around the player (for wider circling)
            Vector3 randomOffset = Random.insideUnitSphere * 150f;  // Adds randomness to movement
            Vector3 targetPosition = player.position + randomOffset;

            // Calculate direction to the new random position
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;

            // Apply rotation towards the target position while rolling
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, Vector3.up);  // Or use right/up for banking
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Add forward movement to continue circling smoothly
            transform.position += transform.forward * speed * Time.deltaTime;

            // Optional: Apply roll for banked turns (based on yaw)
            float rollAngle = Mathf.Clamp(Vector3.Dot(transform.right, directionToTarget), -1f, 1f) * 45f;  // Roll angle based on direction
            transform.Rotate(Vector3.forward, rollAngle * Time.deltaTime);

            // Add vertical movement (to get under and over the player)
            float verticalOffset = Mathf.Sin(Time.time * Random.Range(0.5f, 1.5f)) * 30f;
            transform.position += Vector3.up * verticalOffset * Time.deltaTime;
        }
        else
        {
            // Smooth rotation towards the player for attack run
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Avoid ramming during approach by slowing down if too close
            if (distanceToPlayer <= shootDistance * 0.5f)
            {
                // Slow down to avoid collision
                transform.position += transform.forward * (speed * 0.5f) * Time.deltaTime;
            }
            else
            {
                // Continue full speed if far enough
                transform.position += transform.forward * speed * Time.deltaTime;
            }
        }
    }


    void BreakOutAndAttack()
    {
        // Move away from the player to reposition
        Vector3 breakOutDirection = (transform.forward - transform.right).normalized;
        transform.rotation = Quaternion.LookRotation(breakOutDirection);
    }

    void HandleShooting(float distanceToPlayer)
    {
        if (rightGun == null)
        {
            Debug.LogWarning("RightGUN is not assigned in the inspector.");
            return;
        }

        // Calculate angle between enemy's forward direction and direction to player
        Vector3 directionToPlayer = (player.position - rightGun.position).normalized;
        float angleToPlayer = Vector3.Angle(rightGun.forward, directionToPlayer);

        // Visualize shooting cone and range
        if (debugRaycast)
        {
            // Draw shooting range
            Debug.DrawRay(rightGun.position, rightGun.forward * shootDistance, Color.red);

            // Draw shooting angle cone
            Vector3 leftLimit = Quaternion.Euler(0, -shootingAngle / 2, 0) * rightGun.forward;
            Vector3 rightLimit = Quaternion.Euler(0, shootingAngle / 2, 0) * rightGun.forward;
            Debug.DrawRay(rightGun.position, leftLimit * shootDistance, Color.yellow);
            Debug.DrawRay(rightGun.position, rightLimit * shootDistance, Color.yellow);
        }

        // Check if the player is within shooting angle and distance
        if (angleToPlayer <= shootingAngle / 2 && distanceToPlayer <= shootDistance)
        {
            float shootInterval = Random.Range(minShootInterval, maxShootInterval);

            if (shotsFired < maxShots && Time.time > lastShotTime + shootInterval)
            {
                ShootMissile();
                lastShotTime = Time.time;
                shotsFired++;
            }

            if (shotsFired >= maxShots)
            {
                reloading = true;
                reloadStartTime = Time.time;
            }
        }
    }

    void ShootMissile()
    {
        Vector3 directionToPlayer = (player.position - rightGun.position).normalized;
        Quaternion missileRotation = Quaternion.LookRotation(directionToPlayer);
        GameObject missile = Instantiate(missilePrefab, rightGun.position, missileRotation);

        Rigidbody missileRb = missile.GetComponent<Rigidbody>();
        missileRb.velocity = directionToPlayer * missileSpeed;

        MissileBehavior missileBehavior = missile.GetComponent<MissileBehavior>();
        missileBehavior.InitializeMissile(500f, explosionEffect); // Pass explosionEffect to missile
        missileBehavior.SetTarget(player); // Ensure the missile targets the player
    }

    void OnDrawGizmos()
    {
        if (rightGun != null)
        {
            Gizmos.color = Color.red;
            // Draw shooting range
            Gizmos.DrawLine(rightGun.position, rightGun.position + rightGun.forward * shootDistance);

            // Draw shooting angle cone
            Gizmos.color = Color.yellow;
            Vector3 leftLimit = Quaternion.Euler(0, -shootingAngle / 2, 0) * rightGun.forward;
            Vector3 rightLimit = Quaternion.Euler(0, shootingAngle / 2, 0) * rightGun.forward;
            Gizmos.DrawLine(rightGun.position, rightGun.position + leftLimit * shootDistance);
            Gizmos.DrawLine(rightGun.position, rightGun.position + rightLimit * shootDistance);

            // Optionally draw an arc to represent the shooting angle
            Gizmos.color = new Color(1f, 1f, 0f, 0.1f); // Semi-transparent yellow
            int segments = 20;
            float angleStep = shootingAngle / segments;
            Vector3 previousPoint = rightGun.position + leftLimit * shootDistance;
            for (int i = 1; i <= segments; i++)
            {
                float currentAngle = -shootingAngle / 2 + angleStep * i;
                Vector3 currentDirection = Quaternion.Euler(0, currentAngle, 0) * rightGun.forward;
                Vector3 currentPoint = rightGun.position + currentDirection * shootDistance;
                Gizmos.DrawLine(previousPoint, currentPoint);
                previousPoint = currentPoint;
            }
        }
    }
}
