using UnityEngine;
using UnityEngine.InputSystem;

namespace LockUp
{
    /// <summary>
    /// First person character: walk / sprint / crouch / jump, mouse + gamepad look.
    /// Reads devices directly through the Input System, so nothing needs wiring in
    /// the inspector. Swap over to InputSystem_Actions later if you want rebinding.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float walkSpeed = 4.5f;
        public float sprintSpeed = 7.5f;
        public float crouchSpeed = 2.2f;
        public float acceleration = 14f;
        public float airControl = 0.35f;
        public float jumpHeight = 1.1f;
        public float gravity = -22f;

        [Header("Look")]
        public Transform cameraPivot;
        public float mouseSensitivity = 0.09f;
        public float gamepadSensitivity = 180f;
        public float minPitch = -85f;
        public float maxPitch = 85f;
        public bool invertY = false;

        [Header("Stance")]
        public float standHeight = 1.8f;
        public float crouchHeight = 1.1f;
        public float stanceSpeed = 10f;

        [Header("Feel")]
        public bool headBob = true;
        public float bobFrequency = 9f;
        public float bobAmplitude = 0.045f;

        CharacterController cc;
        Vector3 planarVelocity;
        float verticalVelocity;
        float pitch;
        float currentHeight;
        float bobTimer;
        float eyeOffset;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            currentHeight = standHeight;
            ApplyHeight(standHeight);

            if (cameraPivot == null)
            {
                Camera cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraPivot = cam.transform;
            }

            eyeOffset = cameraPivot != null ? cameraPivot.localPosition.y - currentHeight : -0.15f;
        }

        void OnEnable()
        {
            LockCursor(true);
        }

        void Update()
        {
            HandleCursor();
            Look();
            Move();
        }

        // ---------------------------------------------------------------- cursor

        void HandleCursor()
        {
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.escapeKey.wasPressedThisFrame) LockCursor(false);

            Mouse mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
                LockCursor(true);
        }

        static void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        // ------------------------------------------------------------------ look

        void Look()
        {
            if (cameraPivot == null) return;

            Vector2 look = Vector2.zero;

            Mouse mouse = Mouse.current;
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
                look += mouse.delta.ReadValue() * mouseSensitivity;

            Gamepad pad = Gamepad.current;
            if (pad != null)
                look += pad.rightStick.ReadValue() * (gamepadSensitivity * Time.deltaTime);

            if (invertY) look.y = -look.y;

            transform.Rotate(Vector3.up, look.x, Space.Self);

            pitch = Mathf.Clamp(pitch - look.y, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // ------------------------------------------------------------------ move

        void Move()
        {
            Vector2 input = ReadMoveInput();
            bool wantsCrouch = HeldCrouch();
            bool sprinting = HeldSprint() && !wantsCrouch && input.y > 0.1f;

            UpdateStance(wantsCrouch);

            float targetSpeed = wantsCrouch ? crouchSpeed : (sprinting ? sprintSpeed : walkSpeed);

            Vector3 wish = transform.right * input.x + transform.forward * input.y;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            float responsiveness = acceleration * (cc.isGrounded ? 1f : airControl);
            planarVelocity = Vector3.MoveTowards(planarVelocity, wish * targetSpeed,
                                                responsiveness * Time.deltaTime);

            if (cc.isGrounded)
            {
                if (verticalVelocity < 0f) verticalVelocity = -2f;
                if (PressedJump() && !wantsCrouch)
                    verticalVelocity = Mathf.Sqrt(-2f * gravity * jumpHeight);
            }
            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion = planarVelocity + Vector3.up * verticalVelocity;
            cc.Move(motion * Time.deltaTime);

            Bob(input.sqrMagnitude > 0.01f && cc.isGrounded, targetSpeed);
        }

        void UpdateStance(bool wantsCrouch)
        {
            float target = wantsCrouch ? crouchHeight : standHeight;

            // Never stand up into geometry.
            if (target > currentHeight && CeilingBlocked(target)) target = currentHeight;

            currentHeight = Mathf.Lerp(currentHeight, target, stanceSpeed * Time.deltaTime);
            ApplyHeight(currentHeight);
        }

        bool CeilingBlocked(float targetHeight)
        {
            float radius = cc.radius * 0.9f;
            Vector3 sphereTop = transform.position + Vector3.up * (targetHeight - radius);

            Collider[] hits = Physics.OverlapSphere(sphereTop, radius, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < hits.Length; i++)
                if (hits[i] != cc) return true;

            return false;
        }

        void ApplyHeight(float h)
        {
            cc.height = h;
            cc.center = new Vector3(0f, h * 0.5f, 0f);
        }

        void Bob(bool moving, float speed)
        {
            if (cameraPivot == null) return;

            float eyeY = currentHeight + eyeOffset;

            if (headBob)
            {
                if (moving) bobTimer += Time.deltaTime * bobFrequency * (speed / walkSpeed);
                else bobTimer = Mathf.MoveTowards(bobTimer, 0f, Time.deltaTime * bobFrequency);

                if (moving) eyeY += Mathf.Sin(bobTimer) * bobAmplitude;
            }

            Vector3 p = cameraPivot.localPosition;
            cameraPivot.localPosition = new Vector3(p.x, Mathf.Lerp(p.y, eyeY, 20f * Time.deltaTime), p.z);
        }

        // ----------------------------------------------------------------- input

        static Vector2 ReadMoveInput()
        {
            Vector2 v = Vector2.zero;

            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
            }

            Gamepad pad = Gamepad.current;
            if (pad != null) v += pad.leftStick.ReadValue();

            return Vector2.ClampMagnitude(v, 1f);
        }

        static bool HeldSprint()
        {
            Keyboard kb = Keyboard.current;
            Gamepad pad = Gamepad.current;
            return (kb != null && kb.leftShiftKey.isPressed) ||
                   (pad != null && pad.leftStickButton.isPressed);
        }

        static bool HeldCrouch()
        {
            Keyboard kb = Keyboard.current;
            Gamepad pad = Gamepad.current;
            return (kb != null && (kb.leftCtrlKey.isPressed || kb.cKey.isPressed)) ||
                   (pad != null && pad.buttonEast.isPressed);
        }

        static bool PressedJump()
        {
            Keyboard kb = Keyboard.current;
            Gamepad pad = Gamepad.current;
            return (kb != null && kb.spaceKey.wasPressedThisFrame) ||
                   (pad != null && pad.buttonSouth.wasPressedThisFrame);
        }
    }
}
