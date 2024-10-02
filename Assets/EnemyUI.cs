using TMPro;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    public string enemyName = "Enemy Ship";  // Set this in the Inspector or dynamically
    public Transform player;                 // Reference to the player
    private CombatHandler combatHandler;     // Reference to the CombatHandler for shield and health values

    private TextMeshProUGUI nameText;
    private TextMeshProUGUI distanceText;
    private TextMeshProUGUI shieldText;
    private TextMeshProUGUI hullText;
    private Transform canvasTransform;       // Transform of the Canvas

    void Start()
    {
        // Get the transform of the Canvas (which should be a child of the prefab)
        canvasTransform = GetComponentInChildren<Canvas>().transform;

        // Find all TextMeshPro children
        foreach (TextMeshProUGUI text in GetComponentsInChildren<TextMeshProUGUI>())
        {
            if (text.gameObject.name == "Name")
                nameText = text;
            else if (text.gameObject.name == "Distance")
                distanceText = text;
            else if (text.gameObject.name == "Shield")
                shieldText = text;
            else if (text.gameObject.name == "Hull")
                hullText = text;
        }

        // Set the enemy name initially
        if (nameText != null)
            nameText.text = enemyName;

        // Get the CombatHandler component from the root of the prefab
        combatHandler = GetComponentInParent<CombatHandler>();
    }

    void Update()
    {
        // Ensure player reference exists
        if (player != null)
        {
            // Calculate the distance to the player
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Update the distance text
            if (distanceText != null)
                distanceText.text = $"Distance: {distanceToPlayer:F1}";
        }

        // Make the canvas always face the camera and stay upright
        if (canvasTransform != null && Camera.main != null)
        {
            Vector3 directionToCamera = Camera.main.transform.position - canvasTransform.position;
            directionToCamera.y = 0; // Flatten the direction on the Y-axis

            Vector3 cameraUp = Camera.main.transform.up;
            canvasTransform.rotation = Quaternion.LookRotation(directionToCamera, cameraUp);
            canvasTransform.Rotate(0, 180, 0); // Fix reversed text
        }

        // Update shield and hull values dynamically
        if (combatHandler != null)
        {
            if (shieldText != null)
            {
                // Calculate shield as a percentage and update the UI
                float shieldPercentage = (combatHandler.currentShield / (float)combatHandler.maxShield) * 100;
                shieldText.text = $"Shield: {shieldPercentage:F1}%";
            }

            if (hullText != null)
            {
                // Update hull value directly as percentage
                float hullPercentage = (combatHandler.currentHealth / (float)combatHandler.maxHealth) * 100;
                hullText.text = $"Hull: {hullPercentage:F1}%";
            }
        }
    }
}
