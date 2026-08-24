using UnityEngine;
using UnityEngine.InputSystem;

// First-person movement + mouse look for the prototype.
// Uses the new Input System (this project has it set as the only active handler).
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.8f;
    public float jumpHeight = 1.4f;
    public float gravity = -18f;
    public float mouseSensitivity = 8f;
    public Transform cameraTransform; // assigned by SceneBuilder to the child camera

    private CharacterController controller;
    private Vector3 verticalVelocity;
    private float pitch = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }

        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        if (cameraTransform == null || Cursor.lockState != CursorLockMode.Locked) return;

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue();
        float mouseX = delta.x * mouseSensitivity * 0.02f;
        float mouseY = delta.y * mouseSensitivity * 0.02f;

        // yaw rotates the whole player body (so movement stays relative to facing)
        transform.Rotate(Vector3.up * mouseX);

        // pitch only rotates the camera
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -85f, 85f);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // read grounded state once, before doing any Move() this frame
        bool grounded = controller.isGrounded;

        if (grounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        if (grounded && keyboard.spaceKey.wasPressedThisFrame)
        {
            // v = sqrt(2 * g * h)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        float x = 0f;
        float z = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;

        // movement is relative to where the player is facing (standard FPS controls)
        Vector3 horizontalMove = transform.right * x + transform.forward * z;
        if (horizontalMove.sqrMagnitude > 1f) horizontalMove.Normalize();

        bool sprinting = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        float currentSpeed = sprinting ? moveSpeed * sprintMultiplier : moveSpeed;

        // combine horizontal + vertical into a single Move() call per frame
        Vector3 fullMove = horizontalMove * currentSpeed + new Vector3(0f, verticalVelocity.y, 0f);
        controller.Move(fullMove * Time.deltaTime);
    }
}
