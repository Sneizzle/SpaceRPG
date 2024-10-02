using UnityEngine;

public class SpaceshipController : MonoBehaviour
{
    // Public configurable variables
    public float maxSpeed = 20f;
    public float acceleration = 10f;
    public float mouseSensitivity = 2f;
    public float rollSpeed = 50f;

    // Dampener setting
    private bool dampenersOn = true;

    // Movement vectors
    private Vector3 velocity;
    private Vector3 inputDirection;

    // Rigidbody for physics-based movement
    private Rigidbody rb;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity in space
        rb.drag = 1f; // Simulates resistance with dampeners on
    }

    void Update()
    {
        // Get input for movement
        float moveForward = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        float moveRight = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
        float moveUp = Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.C) ? -1 : 0;

        inputDirection = new Vector3(moveRight, moveUp, moveForward);

        // Roll with Q and E
        float roll = (Input.GetKey(KeyCode.Q) ? 1 : Input.GetKey(KeyCode.E) ? -1 : 0) * rollSpeed * Time.deltaTime;

        // Toggle dampeners
        if (Input.GetKeyDown(KeyCode.Z))
        {
            dampenersOn = !dampenersOn;
            rb.drag = dampenersOn ? 1f : 0f;
        }

        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Apply rotation based on mouse input
        transform.Rotate(mouseY, mouseX, roll);

        // Limit speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }

    void FixedUpdate()
    {
        // Apply force for movement with acceleration
        if (inputDirection != Vector3.zero)
        {
            rb.AddRelativeForce(inputDirection * acceleration);
        }

        // Apply dampeners to slow down when no input is given
        if (dampenersOn && inputDirection == Vector3.zero)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime);
        }
    }
}
