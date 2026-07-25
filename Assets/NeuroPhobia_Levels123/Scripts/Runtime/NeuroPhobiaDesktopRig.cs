using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class NeuroPhobiaDesktopRig : MonoBehaviour
{
    [Header("Referencias")]
    public Camera desktopCamera;

    [Header("Movimiento")]
    public float walkSpeed = 2.2f;
    public float sprintSpeed = 4.0f;
    public float mouseSensitivity = 0.12f;
    public float gravity = -18f;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool cursorLocked = true;

    private void Awake()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        controller = GetComponent<CharacterController>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        if (desktopCamera == null) desktopCamera = GetComponentInChildren<Camera>(true);
        ActivateDesktopMode();
#else
        gameObject.SetActive(false);
#endif
    }

    private void ActivateDesktopMode()
    {
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            if (root == gameObject) continue;
            string n = root.name.ToLowerInvariant();
            if (n.Contains("xr origin") || n.Contains("xr device simulator")) root.SetActive(false);
        }

        if (desktopCamera != null)
        {
            desktopCamera.gameObject.SetActive(true);
            desktopCamera.tag = "MainCamera";
            foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (canvas.renderMode == RenderMode.WorldSpace) canvas.worldCamera = desktopCamera;
            }
        }

        SetCursorLock(true);
    }

    private void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Keyboard.current == null || Mouse.current == null || controller == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame) SetCursorLock(!cursorLocked);
        if (Keyboard.current.rKey.wasPressedThisFrame) ResetPosition();
        if (Keyboard.current.enterKey.wasPressedThisFrame) InvokeVisibleContinueButton();

        if (cursorLocked) UpdateLook();
        UpdateMovement();
#endif
    }

    private void UpdateLook()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        transform.Rotate(Vector3.up, delta.x, Space.World);
        pitch = Mathf.Clamp(pitch - delta.y, -80f, 80f);
        if (desktopCamera != null) desktopCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void UpdateMovement()
    {
        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) input.y += 1f;
        if (Keyboard.current.sKey.isPressed) input.y -= 1f;
        if (Keyboard.current.dKey.isPressed) input.x += 1f;
        if (Keyboard.current.aKey.isPressed) input.x -= 1f;
        input = Vector2.ClampMagnitude(input, 1f);

        float speed = Keyboard.current.leftShiftKey.isPressed ? sprintSpeed : walkSpeed;
        Vector3 horizontal = (transform.forward * input.y + transform.right * input.x) * speed;
        if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -1f;
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move((horizontal + Vector3.up * verticalVelocity) * Time.deltaTime);
    }

    private void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void ResetPosition()
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        controller.enabled = true;
        verticalVelocity = 0f;
        pitch = 0f;
    }

    private void InvokeVisibleContinueButton()
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (!button.interactable || !button.gameObject.activeInHierarchy) continue;
            string n = button.name.ToLowerInvariant();
            if (n.Contains("continuar") || n.Contains("continue"))
            {
                button.onClick.Invoke();
                return;
            }
        }
    }
}
