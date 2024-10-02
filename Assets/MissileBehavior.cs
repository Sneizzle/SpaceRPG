using UnityEngine;

public class MissileBehavior : MonoBehaviour
{
    public float explosionRadius = 5f;         // Radius of the explosion
    public int damage = 4;                     // Damage caused by the missile
    private Vector3 startPosition;             // The initial position of the missile
    private float maxTravelDistance;           // Distance before explosion
    private bool isExploded = false;           // Flag to ensure it explodes once
    public GameObject explosionEffect;         // Effect for explosion
    private Rigidbody rb;

    private Transform target;                  // The target the missile is heading towards

    void Awake()
    {
        rb = GetComponent<Rigidbody>();  // Ensure Rigidbody is assigned at the start
    }

    public void InitializeMissile(float explosionDistance, GameObject effect)
    {
        startPosition = transform.position;
        maxTravelDistance = explosionDistance;
        explosionEffect = effect;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;

        // Calculate the initial direction to the target at the moment of firing
        Vector3 directionToTarget = (target.position - transform.position).normalized;

        // Set the missile's velocity once, at the time of firing
        rb.velocity = directionToTarget * rb.velocity.magnitude;
    }

    void Update()
    {
        // Calculate the distance traveled by the missile
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        // If the missile has traveled the max distance without hitting anything, explode
        if (distanceTraveled >= maxTravelDistance && !isExploded)
        {
            Explode();
        }
    }




    private void OnCollisionEnter(Collision collision)
    {
        if (!isExploded)
        {
            CombatHandler enemyCombatHandler = collision.gameObject.GetComponent<CombatHandler>();

            if (enemyCombatHandler != null)
            {
                enemyCombatHandler.TakeDamage(damage);
            }

            Explode();
        }
    }

    void Explode()
    {
        isExploded = true;

        if (explosionEffect != null)
        {
            GameObject explosionInstance = Instantiate(explosionEffect, transform.position, transform.rotation);

            ParticleSystem ps = explosionInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(explosionInstance, ps.main.duration);
            }
            else
            {
                Destroy(explosionInstance, 2f);
            }
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(1000f, transform.position, explosionRadius);
            }
        }

        Destroy(gameObject);
    }
}
