using UnityEngine;

public class Level5SpiderRoam : MonoBehaviour
{
    public float roamRadius = 1.2f;
    public float moveSpeed = 0.15f;
    public float changeTargetSeconds = 3f;

    private Vector3 origin;
    private Vector3 target;
    private float timer;

    private void Start()
    {
        origin = transform.position;
        PickTarget();
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f || Vector3.Distance(transform.position, target) < 0.05f)
            PickTarget();

        Vector3 direction = target - transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                2f * Time.deltaTime
            );
        }
    }

    private void PickTarget()
    {
        Vector2 point = Random.insideUnitCircle * roamRadius;
        target = origin + new Vector3(point.x, 0f, point.y);
        timer = changeTargetSeconds;
    }
}
