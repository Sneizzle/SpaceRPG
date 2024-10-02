using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage = 2f;  // Set damage per bullet

    void OnCollisionEnter(Collision collision)
    {
        // Check if the bullet hit something with a CombatHandler
        CombatHandler combatHandler = collision.gameObject.GetComponent<CombatHandler>();
        if (combatHandler != null)
        {
            combatHandler.TakeDamage((int)damage);  // Apply damage
        }

        // Destroy the bullet on impact
        Destroy(gameObject);
    }
}
