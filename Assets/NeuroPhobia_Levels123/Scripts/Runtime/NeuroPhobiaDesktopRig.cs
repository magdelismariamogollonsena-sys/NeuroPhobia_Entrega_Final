using UnityEngine;

/// <summary>
/// Rig de escritorio heredado. NeuroPhobia es exclusivamente VR (Meta Quest 3):
/// este componente ya no procesa teclado/mouse ni desactiva el XR Origin.
/// Se conserva la clase para no romper referencias serializadas en las escenas.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class NeuroPhobiaDesktopRig : MonoBehaviour
{
    [Header("Referencias (sin uso, VR-only)")]
    public Camera desktopCamera;

    private void Awake()
    {
        gameObject.SetActive(false);
    }
}
