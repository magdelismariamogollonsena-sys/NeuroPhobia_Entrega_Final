using UnityEngine;

public interface IPhobiaStimulus
{
    void Pause();
    void Resume();
    float GetDistanceToPlayer();
    void SetMovementPattern(MovementPattern pattern);
}

public enum MovementPattern
{
    IDLE,
    SLOW_PATROL,
    HAND_CRAWL,
    FREE_ROAM
}

public class AranaController : MonoBehaviour, IPhobiaStimulus
{
    public MovementPattern defaultPattern = MovementPattern.IDLE;
    public Transform player;
    public Transform handTarget;
    public float moveSpeed = 0.35f;
    public float patrolRadius = 1.5f;

    private MovementPattern currentPattern;
    private bool paused;
    private Vector3 startPosition;
    private float patrolTimer;

    private void Start()
    {
        startPosition = transform.position;
        currentPattern = defaultPattern;
        if (player == null && Camera.main != null) player = Camera.main.transform;
    }

    private void Update()
    {
        if (paused) return;

        switch (currentPattern)
        {
            case MovementPattern.SLOW_PATROL:
                PatrolSlowly();
                break;
            case MovementPattern.HAND_CRAWL:
                CrawlToHand();
                break;
            case MovementPattern.FREE_ROAM:
                FreeRoam();
                break;
        }
    }

    public void Pause() { paused = true; }
    public void Resume() { paused = false; }

    public float GetDistanceToPlayer()
    {
        if (player == null) return float.MaxValue;
        return Vector3.Distance(transform.position, player.position);
    }

    public void SetMovementPattern(MovementPattern pattern)
    {
        currentPattern = pattern;
    }

    private void PatrolSlowly()
    {
        patrolTimer += Time.deltaTime * moveSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(patrolTimer) * patrolRadius, 0f, Mathf.Cos(patrolTimer) * patrolRadius);
        transform.position = startPosition + offset;
    }

    private void CrawlToHand()
    {
        if (handTarget == null) return;
        transform.position = Vector3.MoveTowards(transform.position, handTarget.position, moveSpeed * Time.deltaTime);
    }

    private void FreeRoam()
    {
        patrolTimer += Time.deltaTime * moveSpeed * 0.5f;
        Vector3 offset = new Vector3(Mathf.Sin(patrolTimer) * patrolRadius, 0f, Mathf.Cos(patrolTimer * 0.7f) * patrolRadius);
        transform.position = startPosition + offset;
    }
}
