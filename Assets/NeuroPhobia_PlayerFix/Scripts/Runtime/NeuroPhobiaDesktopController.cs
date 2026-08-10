using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Control de escritorio común para NeuroPhobia.
/// Soporta WASD/flechas, mouse vertical y horizontal, Shift y Escape.
/// Compatible con Input System, Input Manager o Both.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public sealed class NeuroPhobiaDesktopController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform cameraPivot;
    public Transform pitchPivot;
    public Camera playerCamera;

    [Header("Movimiento")]
    public float moveSpeed = 2.8f;
    public float runMultiplier = 1.55f;
    public float gravity = -18f;

    [Header("Mouse")]
    public float mouseSensitivityX = 0.10f;
    public float mouseSensitivityY = 0.10f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public bool lockCursorOnEnable = true;

    // Compatibilidad con instaladores anteriores de NeuroPhobia.
    // El instalador viejo escribía controller.mouseSensitivity.
    // Esta propiedad actualiza ambos ejes para no romper esos scripts.
    public float mouseSensitivity
    {
        get => (mouseSensitivityX + mouseSensitivityY) * 0.5f;
        set
        {
            mouseSensitivityX = value;
            mouseSensitivityY = value;
        }
    }

    private CharacterController characterController;
    private float pitch;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        ResolveReferences();
        ReadInitialPitch();
    }

    private void OnEnable()
    {
        ResolveReferences();
        ReadInitialPitch();

        if (lockCursorOnEnable && Application.isPlaying)
        {
            LockCursor();
        }
    }

    private void Update()
    {
        if (ReadEscapePressed())
        {
            UnlockCursor();
        }

        if (ReadPrimaryClickPressed() &&
            Cursor.lockState != CursorLockMode.Locked)
        {
            LockCursor();
        }

        HandleLook();
        HandleMovement();
    }

    private void ResolveReferences()
    {
        if (cameraPivot == null)
        {
            Transform found = transform.Find("CameraPivot");
            if (found != null)
            {
                cameraPivot = found;
            }
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (pitchPivot == null)
        {
            Transform root = cameraPivot != null ? cameraPivot : transform;
            Transform found = FindRecursive(root, "NP_MousePitchPivot");
            if (found != null)
            {
                pitchPivot = found;
            }
        }

        if (pitchPivot == null && playerCamera != null)
        {
            pitchPivot = playerCamera.transform;
        }
    }

    private void ReadInitialPitch()
    {
        Transform target = GetPitchTarget();
        if (target == null)
        {
            pitch = 0f;
            return;
        }

        float x = target.localEulerAngles.x;
        pitch = x > 180f ? x - 360f : x;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private Transform GetPitchTarget()
    {
        if (pitchPivot != null)
        {
            return pitchPivot;
        }

        if (cameraPivot != null)
        {
            return cameraPivot;
        }

        return playerCamera != null ? playerCamera.transform : null;
    }

    private void HandleLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            return;
        }

        Vector2 look = ReadLook();

        transform.Rotate(
            0f,
            look.x * mouseSensitivityX,
            0f,
            Space.Self);

        pitch = Mathf.Clamp(
            pitch - look.y * mouseSensitivityY,
            minPitch,
            maxPitch);

        Transform target = GetPitchTarget();
        if (target != null)
        {
            target.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        if (characterController == null)
        {
            return;
        }

        Vector2 input = Vector2.ClampMagnitude(ReadMove(), 1f);

        Vector3 planar =
            transform.right * input.x +
            transform.forward * input.y;

        float speed =
            moveSpeed *
            (ReadRunHeld() ? runMultiplier : 1f);

        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 motion =
            planar * speed * Time.deltaTime +
            Vector3.up * verticalVelocity * Time.deltaTime;

        characterController.Move(motion);
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

    private static Vector2 ReadMove()
    {
        Vector2 value = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
        }

        if (value.sqrMagnitude > 0.001f)
        {
            return Vector2.ClampMagnitude(value, 1f);
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        value = new Vector2(
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
            Vector2 delta = Mouse.current.delta.ReadValue();
            if (delta.sqrMagnitude > 0.000001f)
            {
                return delta;
            }
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

    private static bool ReadPrimaryClickPressed()
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

    private static bool ReadEscapePressed()
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

    private static bool ReadRunHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null &&
            (keyboard.leftShiftKey.isPressed ||
             keyboard.rightShiftKey.isPressed))
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKey(KeyCode.LeftShift) ||
               Input.GetKey(KeyCode.RightShift);
#else
        return false;
#endif
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName)
    {
        if (current == null)
        {
            return null;
        }

        if (current.name == exactName)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform found = FindRecursive(child, exactName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
