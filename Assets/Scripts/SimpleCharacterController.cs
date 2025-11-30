using UnityEngine;

/// <summary>
/// Basic first-person style character controller that uses Unity's CharacterController
/// component for collision. Supports walking, running, jumping, gravity, and simple camera
/// look controls driven by mouse input.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Base walk speed in meters per second.")]
    [SerializeField] private float walkSpeed = 5f;

    [Tooltip("Multiplier applied when the run key is held.")]
    [SerializeField] private float runMultiplier = 1.6f;

    [Tooltip("Acceleration applied when changing velocity.")]
    [SerializeField] private float acceleration = 12f;

    [Header("Jumping & Gravity")]
    [Tooltip("Upward velocity applied when the player jumps.")]
    [SerializeField] private float jumpStrength = 6.5f;

    [Tooltip("Additional downward acceleration (positive value).")]
    [SerializeField] private float gravity = 20f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraRoot;

    [Tooltip("Mouse sensitivity for horizontal and vertical look.")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(2.5f, 2.5f);

    [Tooltip("Clamp for vertical look (degrees).")]
    [SerializeField] private Vector2 verticalLookLimits = new Vector2(-80f, 80f);

    [Header("Input")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null && Camera.main != null)
        {
            cameraRoot = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity.x;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity.y;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, verticalLookLimits.x, verticalLookLimits.y);

        if (cameraRoot != null)
        {
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);

        bool isRunning = Input.GetKey(runKey);
        float targetSpeed = walkSpeed * (isRunning ? runMultiplier : 1f);
        Vector3 targetVelocity = transform.TransformDirection(input) * targetSpeed;

        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * Time.deltaTime);
        velocity = new Vector3(horizontalVelocity.x, velocity.y, horizontalVelocity.z);

        if (controller.isGrounded)
        {
            velocity.y = -2f; // keeps the controller grounded

            if (Input.GetKeyDown(jumpKey))
            {
                velocity.y = jumpStrength;
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
