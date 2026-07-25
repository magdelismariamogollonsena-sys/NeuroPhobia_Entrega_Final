using UnityEngine;

/// <summary>
/// Da vida sutil a la araña del Nivel 2 sin desplazar su raíz.
/// Si existe una animación Idle la reproduce lentamente; si no, usa un movimiento procedural mínimo.
/// </summary>
public sealed class Level2SpiderIdleLife : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator animator;
    [SerializeField] private bool useAnimator;
    [SerializeField] private float animatorSpeed = 0.22f;
    [SerializeField] private float verticalAmplitude = 0.0035f;
    [SerializeField] private float rotationAmplitude = 0.7f;
    [SerializeField] private float frequency = 0.65f;

    private Vector3 rootPosition;
    private Quaternion rootRotation;
    private Vector3 visualLocalPosition;
    private Quaternion visualLocalRotation;

    private void Start()
    {
        rootPosition = transform.position;
        rootRotation = transform.rotation;
        if (visualRoot == null && transform.childCount > 0) visualRoot = transform.GetChild(0);
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (visualRoot != null)
        {
            visualLocalPosition = visualRoot.localPosition;
            visualLocalRotation = visualRoot.localRotation;
        }
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.speed = useAnimator ? animatorSpeed : 0f;
            animator.enabled = useAnimator;
        }
    }

    private void LateUpdate()
    {
        transform.SetPositionAndRotation(rootPosition, rootRotation);
        if (useAnimator || visualRoot == null) return;
        float wave = Mathf.Sin(Time.time * frequency * Mathf.PI * 2f);
        float slow = Mathf.Sin(Time.time * frequency * 0.43f * Mathf.PI * 2f);
        visualRoot.localPosition = visualLocalPosition + Vector3.up * (wave * verticalAmplitude);
        visualRoot.localRotation = visualLocalRotation * Quaternion.Euler(slow * rotationAmplitude, wave * rotationAmplitude * 0.35f, 0f);
    }
}
