using UnityEngine;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;

public class SitTrigger : MonoBehaviour
{
    [Header("Botón B del mando derecho")]
    public InputActionProperty botonB;

    [Header("Altura al sentarse")]
    public float alturaSentado = 0.5f;

    private GameObject locomotion;
    private Transform cameraOffset;

    private Vector3 posicionOriginal;
    private bool sentado = false;

    private void Start()
    {
        XROrigin xrOrigin = FindFirstObjectByType<XROrigin>();

        if (xrOrigin != null)
        {
            // Buscar Locomotion
            Transform locomotionTransform = xrOrigin.transform.Find("Locomotion");

            if (locomotionTransform != null)
            {
                locomotion = locomotionTransform.gameObject;
            }

            // Buscar Camera Offset
            cameraOffset = xrOrigin.transform.Find("Camera Offset");

            if (cameraOffset != null)
            {
                posicionOriginal = cameraOffset.localPosition;
            }
        }
    }

    private void OnEnable()
    {
        botonB.action.Enable();
    }

    private void OnDisable()
    {
        botonB.action.Disable();
    }

    private void Update()
    {
        if (sentado && botonB.action.WasPressedThisFrame())
        {
            Levantarse();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Sentarse();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Al salir del trigger, se levanta automáticamente
        Levantarse();
    }

    private void Sentarse()
    {
        sentado = true;

        // Bloquear movimiento
        if (locomotion != null)
        {
            locomotion.SetActive(false);
        }

        // Bajar cámara
        if (cameraOffset != null)
        {
            Vector3 nuevaPosicion = posicionOriginal;
            nuevaPosicion.y -= alturaSentado;

            cameraOffset.localPosition = nuevaPosicion;
        }

        Debug.Log("Jugador sentado. Presiona B para levantarte.");
    }

    private void Levantarse()
    {
        sentado = false;

        // Restaurar movimiento
        if (locomotion != null)
        {
            locomotion.SetActive(true);
        }

        // Restaurar cámara a su posición ORIGINAL
        if (cameraOffset != null)
        {
            cameraOffset.localPosition = posicionOriginal;
        }

        Debug.Log("Jugador se levantó.");
    }
}