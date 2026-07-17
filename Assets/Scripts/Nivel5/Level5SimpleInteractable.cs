using UnityEngine;

public class Level5SimpleInteractable : MonoBehaviour
{
    public Level5TaskObject taskObject;
    public Transform movablePart;
    public Vector3 completedLocalEuler;
    public float animationSpeed = 120f;

    private bool activated;
    private Quaternion startRotation;
    private Quaternion completedRotation;

    private void Start()
    {
        if (movablePart == null)
            movablePart = transform;

        startRotation = movablePart.localRotation;
        completedRotation = Quaternion.Euler(completedLocalEuler);
    }

    private void Update()
    {
        if (!activated) return;

        movablePart.localRotation = Quaternion.RotateTowards(
            movablePart.localRotation,
            completedRotation,
            animationSpeed * Time.deltaTime
        );
    }

    public void Activate()
    {
        if (activated) return;

        activated = true;

        if (taskObject != null)
            taskObject.CompleteTask();
    }

    public void ResetInteraction()
    {
        activated = false;
        movablePart.localRotation = startRotation;
    }
}
