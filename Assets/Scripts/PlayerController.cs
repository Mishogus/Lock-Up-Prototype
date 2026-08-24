using UnityEngine;
using UnityEngine.InputSystem;

// Basic WASD/arrow-key movement for the prototype.
// Uses the new Input System (this project has it set as the only active handler).
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float x = 0f;
        float z = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z += 1f;

        Vector3 move = new Vector3(x, 0f, z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        controller.Move(move * moveSpeed * Time.deltaTime);

        // simple gravity so the capsule stays grounded on the floor
        if (controller.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        controller.Move(verticalVelocity * Time.deltaTime);

        // face the direction we're moving
        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(move);
        }
    }
}
