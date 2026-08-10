using UnityEngine;
using UnityEngine.XR;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NeuroPhobia.Level45
{
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public sealed class NPDesktopFallbackPlayer : MonoBehaviour
    {
        public Camera playerCamera;
        public float moveSpeed = 3.2f;
        public float runMultiplier = 1.55f;
        public float mouseSensitivity = 0.10f;
        public float gravity = -18f;
        public bool lockCursorOnStart = true;
        public bool allowArrowKeys = true;

        private CharacterController controller;
        private float pitch;
        private float verticalVelocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>(true);
            }

            if (playerCamera != null)
            {
                Vector3 cameraAngles =
                    playerCamera.transform.localEulerAngles;

                pitch = cameraAngles.x > 180f
                    ? cameraAngles.x - 360f
                    : cameraAngles.x;
            }
        }

        private void Start()
        {
            if (lockCursorOnStart && !XRSettings.isDeviceActive)
            {
                LockCursor();
            }
        }

        private void Update()
        {
            if (XRSettings.isDeviceActive)
            {
                return;
            }

            if (ReadEscape())
            {
                UnlockCursor();
            }

            if (ReadPrimaryClick())
            {
                LockCursor();
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 look = ReadLook();

                transform.Rotate(
                    0f,
                    look.x * mouseSensitivity,
                    0f,
                    Space.Self);

                pitch = Mathf.Clamp(
                    pitch - look.y * mouseSensitivity,
                    -80f,
                    80f);

                if (playerCamera != null)
                {
                    playerCamera.transform.localRotation =
                        Quaternion.Euler(pitch, 0f, 0f);
                }
            }

            Vector2 moveInput = ReadMove(allowArrowKeys);

            Vector3 horizontal =
                transform.right * moveInput.x +
                transform.forward * moveInput.y;

            horizontal = Vector3.ClampMagnitude(
                horizontal,
                1f);

            float speed = moveSpeed *
                (ReadRun() ? runMultiplier : 1f);

            if (controller.isGrounded &&
                verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;

            Vector3 motion =
                horizontal * speed * Time.deltaTime +
                Vector3.up * verticalVelocity * Time.deltaTime;

            controller.Move(motion);
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private static Vector2 ReadMove(bool includeArrowKeys)
        {
            Vector2 value = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed) value.x -= 1f;
                if (keyboard.dKey.isPressed) value.x += 1f;
                if (keyboard.sKey.isPressed) value.y -= 1f;
                if (keyboard.wKey.isPressed) value.y += 1f;

                if (includeArrowKeys)
                {
                    if (keyboard.leftArrowKey.isPressed) value.x -= 1f;
                    if (keyboard.rightArrowKey.isPressed) value.x += 1f;
                    if (keyboard.downArrowKey.isPressed) value.y -= 1f;
                    if (keyboard.upArrowKey.isPressed) value.y += 1f;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            value += new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical"));
#endif

            return Vector2.ClampMagnitude(value, 1f);
        }

        private static Vector2 ReadLook()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                return Mouse.current.delta.ReadValue();
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(
                Input.GetAxis("Mouse X") * 10f,
                Input.GetAxis("Mouse Y") * 10f);
#else
            return Vector2.zero;
#endif
        }

        private static bool ReadEscape()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private static bool ReadPrimaryClick()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        private static bool ReadRun()
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                return keyboardShiftPressed(Keyboard.current);
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift) ||
                   Input.GetKey(KeyCode.RightShift);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static bool keyboardShiftPressed(Keyboard keyboard)
        {
            return keyboard.leftShiftKey.isPressed ||
                   keyboard.rightShiftKey.isPressed;
        }
#endif
    }
}
