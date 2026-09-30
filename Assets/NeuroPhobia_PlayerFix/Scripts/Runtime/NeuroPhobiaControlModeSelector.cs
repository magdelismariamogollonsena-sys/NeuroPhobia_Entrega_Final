using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Bootstrap de jugador para NeuroPhobia VR-only (Meta Quest 3).
/// Activa el XR Origin de la escena y desactiva cualquier rig de escritorio heredado.
/// No hay selección de modo ni entrada de teclado/mouse: el loader XR se
/// inicializa solo (Automatic Loading/Running) y este componente únicamente
/// deja la cámara y el AudioListener correctos en el XR Origin.
/// </summary>
[DefaultExecutionOrder(-20000)]
[DisallowMultipleComponent]
public sealed class NeuroPhobiaControlModeSelector : MonoBehaviour
{
    public enum ControlMode
    {
        None,
        Computer,
        VirtualReality
    }

    [Header("Referencias")]
    public GameObject desktopPlayer;
    public GameObject xrOrigin;
    public Camera desktopCamera;

    [Header("Sin uso, VR-only")]
    public bool askEveryTimeSceneStarts = true;
    public float inputGuardSeconds = 0.45f;

    public ControlMode CurrentMode { get; private set; } = ControlMode.None;

    private static readonly string[] DesktopNames =
    {
        "NP_DesktopPlayer_Normalizado",
        "NP_DesktopPlayer_Niveles45",
        "DesktopTestRig"
    };

    private static readonly string[] XRNames =
    {
        "XR Origin (XR Rig)",
        "XR Origin",
        "XROrigin"
    };

    private void Awake()
    {
        ResolveSceneObjects();
        ActivateVirtualReality();
    }

    private void ActivateVirtualReality()
    {
        if (desktopPlayer != null)
        {
            desktopPlayer.SetActive(false);
        }

        if (xrOrigin == null)
        {
            Debug.LogError("NeuroPhobia: no se encontró XR Origin en esta escena.");
            return;
        }

        xrOrigin.SetActive(true);

        Camera xrCamera = xrOrigin.GetComponentInChildren<Camera>(true);

        if (xrCamera == null)
        {
            Debug.LogError("NeuroPhobia: XR Origin no contiene una cámara.");
            return;
        }

        xrCamera.gameObject.SetActive(true);
        xrCamera.enabled = true;
        EnsureMainCameraTag(xrCamera);
        SetAudioListenerState(xrOrigin, xrCamera);

        CurrentMode = ControlMode.VirtualReality;
    }

    private void ResolveSceneObjects()
    {
        Scene scene = gameObject.scene;

        if (desktopPlayer == null)
        {
            desktopPlayer = FindSceneObjectByAnyName(scene, DesktopNames);
        }

        if (xrOrigin == null)
        {
            xrOrigin = FindSceneObjectByAnyName(scene, XRNames);
        }

        if (desktopCamera == null && desktopPlayer != null)
        {
            desktopCamera = desktopPlayer.GetComponentInChildren<Camera>(true);
        }
    }

    private static GameObject FindSceneObjectByAnyName(
        Scene scene,
        string[] names)
    {
        foreach (string exactName in names)
        {
            GameObject found = FindSceneObject(scene, exactName);

            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindSceneObject(
        Scene scene,
        string exactName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindRecursive(root.transform, exactName);

            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    private static Transform FindRecursive(
        Transform current,
        string exactName)
    {
        if (current.name == exactName)
        {
            return current;
        }

        foreach (Transform child in current)
        {
            Transform result = FindRecursive(child, exactName);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void SetAudioListenerState(
        GameObject root,
        Camera selectedCamera)
    {
        if (root == null)
        {
            return;
        }

        AudioListener[] listeners =
            root.GetComponentsInChildren<AudioListener>(true);

        foreach (AudioListener listener in listeners)
        {
            listener.enabled =
                selectedCamera != null &&
                listener.gameObject == selectedCamera.gameObject;
        }

        if (selectedCamera != null &&
            selectedCamera.GetComponent<AudioListener>() == null)
        {
            selectedCamera.gameObject.AddComponent<AudioListener>();
        }
    }

    private static void EnsureMainCameraTag(Camera camera)
    {
        if (camera == null)
        {
            return;
        }

        try
        {
            camera.gameObject.tag = "MainCamera";
        }
        catch
        {
        }
    }
}
