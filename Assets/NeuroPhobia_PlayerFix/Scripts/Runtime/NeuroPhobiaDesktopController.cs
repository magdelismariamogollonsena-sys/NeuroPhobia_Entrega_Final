using UnityEngine;

/// <summary>
/// Rig de escritorio heredado. NeuroPhobia es exclusivamente VR (Meta Quest 3):
/// este componente ya no procesa teclado/mouse y permanece deshabilitado.
/// Se conserva la clase para no romper referencias serializadas en las escenas.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[DisallowMultipleComponent]
public sealed class NeuroPhobiaDesktopController : MonoBehaviour
{
    [Header("Referencias (sin uso, VR-only)")]
    public Transform cameraPivot;
    public Transform pitchPivot;
    public Camera playerCamera;
    public float moveSpeed = 2.8f;
    public float runMultiplier = 1.55f;
    public float gravity = -18f;
    public float mouseSensitivityX = 0.10f;
    public float mouseSensitivityY = 0.10f;
    public float minPitch = -80f;
    public float maxPitch = 80f;
    public bool lockCursorOnEnable = true;

    public float mouseSensitivity
    {
        get => (mouseSensitivityX + mouseSensitivityY) * 0.5f;
        set
        {
            mouseSensitivityX = value;
            mouseSensitivityY = value;
        }
    }

    public void LockCursor()
    {
    }

    public void UnlockCursor()
    {
    }

    private void Awake()
    {
        enabled = false;
    }
}
