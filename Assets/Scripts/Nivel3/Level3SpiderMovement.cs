using UnityEngine;

public class Level3SpiderMovement : MonoBehaviour
{
    public float moveSpeed = 0.28f;
    public float patrolDistance = 1.2f;
    public float turnSpeed = 1.5f;
    private Vector3 startPosition;
    private bool paused;
    private bool reducedSpeed;
    private float timer;

    private void Start() => startPosition = transform.position;

    private void Update()
    {
        if (paused) return;
        float mult = reducedSpeed ? 0.5f : 1f;
        timer += Time.deltaTime * moveSpeed * mult;
        Vector3 target = startPosition + new Vector3(Mathf.Sin(timer) * patrolDistance, 0f, Mathf.Cos(timer * 0.65f) * patrolDistance * 0.45f);
        Vector3 direction = target - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * mult * Time.deltaTime);
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    public void SetReducedSpeed(bool value) => reducedSpeed = value;
    public void PauseMovement() => paused = true;
    public void ResumeMovement() => paused = false;
}
