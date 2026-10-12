using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Level5TaskInteraction : MonoBehaviour
{
    public string taskId;
    public TaskManager taskManager;
    public Transform movingPart;
    public Transform hingePivot;
    public Vector3 openEuler = new Vector3(0, 70, 0);
    public Transform secondMovingPart;
    public Transform secondHingePivot;
    public Vector3 secondOpenEuler = new Vector3(0, -80, 0);
    public float animationSpeed = 120f;
    public Light controlledLight;
    private Quaternion targetRotation;
    private Quaternion secondTargetRotation;
    private float currentHingeAngle;
    private float currentSecondHingeAngle;
    private bool activated;

    private void Awake()
    {
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null) interactable.selectEntered.AddListener(_ => Activate());
        if (movingPart != null) targetRotation = movingPart.localRotation * Quaternion.Euler(openEuler);
        if (secondMovingPart != null) secondTargetRotation = secondMovingPart.localRotation * Quaternion.Euler(secondOpenEuler);
    }

    private void Update()
    {
        if (!activated || GameStateManager.CurrentState != GameState.PLAYING) return;
        Swing(movingPart, hingePivot, targetRotation, openEuler.y, ref currentHingeAngle);
        Swing(secondMovingPart, secondHingePivot, secondTargetRotation, secondOpenEuler.y, ref currentSecondHingeAngle);
    }

    private void Swing(Transform part, Transform pivot, Quaternion target, float angle, ref float currentAngle)
    {
        if (part == null) return;
        if (pivot == null)
        {
            part.localRotation = Quaternion.RotateTowards(part.localRotation, target, animationSpeed * Time.deltaTime);
            return;
        }
        float next = Mathf.MoveTowards(currentAngle, angle, animationSpeed * Time.deltaTime);
        part.RotateAround(pivot.position, Vector3.up, next - currentAngle);
        currentAngle = next;
    }

    private void OnMouseDown() => Activate();

    public void Activate()
    {
        if (activated || GameStateManager.CurrentState != GameState.PLAYING) return;
        activated = true;
        if (controlledLight != null) controlledLight.enabled = !controlledLight.enabled;
        if (taskManager != null) taskManager.CompleteTask(taskId);
    }
}
