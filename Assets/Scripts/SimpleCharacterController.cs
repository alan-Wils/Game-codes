using UnityEngine;

/// <summary>
/// Basic first-person style character controller that uses Unity's CharacterController
/// component for collision. Supports walking, running, jumping, gravity, and camera look
/// controls driven by mouse input. Can optionally drive a third-person follow camera that
/// hovers behind the player.
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

    [Header("Third-Person Camera")]
    [Tooltip("Enable a hovering, third-person camera that follows behind the player.")]
    [SerializeField] private bool useThirdPersonCamera = false;

    [Tooltip("Local-space offset from the player when using the third-person camera.")]
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0f, 2f, -4f);

    [Tooltip("Height above the player that the camera will look towards.")]
    [SerializeField] private float thirdPersonLookHeight = 1.5f;

    [Tooltip("Smooth time for camera follow (seconds).")]
    [SerializeField] private float cameraFollowSmoothTime = 0.08f;

    [Header("Animation")]
    [Tooltip("Animator that controls the character's animations.")]
    [SerializeField] private Animator animator;

    [Tooltip("Float parameter used to drive locomotion speed (e.g. a blend tree).")]
    [SerializeField] private string speedParameter = "Speed";

    [Tooltip("Damping for smoothing the speed parameter updates.")]
    [SerializeField] private float speedDampTime = 0.1f;

    [Tooltip("Optional bool parameter set true while the character is moving.")]
    [SerializeField] private string walkingBoolParameter = "";

    [Tooltip("Speed (m/s) above which the character is considered walking.")]
    [SerializeField] private float walkSpeedThreshold = 0.1f;

    [Header("Animation Triggers")]
    [Tooltip("Optional trigger fired once when movement starts.")]
    [SerializeField] private string startWalkingTrigger = "";

    [Tooltip("Optional trigger fired once when movement stops.")]
    [SerializeField] private string stopWalkingTrigger = "";

    [Header("Input")]
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;
    private Vector3 cameraFollowVelocity;
    private bool wasMoving;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (cameraRoot == null && Camera.main != null)
        {
            cameraRoot = Camera.main.transform;
        }

        if (cameraRoot != null && useThirdPersonCamera)
        {
            Quaternion cameraRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            cameraRoot.position = transform.position + cameraRotation * thirdPersonOffset;
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
            if (useThirdPersonCamera)
            {
                Quaternion cameraRotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);
                Vector3 targetPosition = transform.position + cameraRotation * thirdPersonOffset;
                cameraRoot.position = Vector3.SmoothDamp(cameraRoot.position, targetPosition, ref cameraFollowVelocity, cameraFollowSmoothTime);

                Vector3 lookTarget = transform.position + Vector3.up * thirdPersonLookHeight;
                cameraRoot.rotation = Quaternion.LookRotation(lookTarget - cameraRoot.position, Vector3.up);
            }
            else
            {
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
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

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null || !animator.isActiveAndEnabled)
        {
            return;
        }

        Vector3 planarVelocity = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        float speed = planarVelocity.magnitude;

        if (!string.IsNullOrEmpty(speedParameter))
        {
            animator.SetFloat(speedParameter, speed, speedDampTime, Time.deltaTime);
        }

        bool isMoving = speed > walkSpeedThreshold;

        if (!string.IsNullOrEmpty(walkingBoolParameter))
        {
            animator.SetBool(walkingBoolParameter, isMoving);
        }

        if (isMoving != wasMoving)
        {
            if (isMoving && !string.IsNullOrEmpty(startWalkingTrigger))
            {
                animator.ResetTrigger(stopWalkingTrigger);
                animator.SetTrigger(startWalkingTrigger);
            }
            else if (!isMoving && !string.IsNullOrEmpty(stopWalkingTrigger))
            {
                animator.ResetTrigger(startWalkingTrigger);
                animator.SetTrigger(stopWalkingTrigger);
            }

            wasMoving = isMoving;
        }
    }
}
