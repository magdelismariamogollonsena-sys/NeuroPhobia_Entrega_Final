using UnityEngine;

public class Level4SpiderHandCrawl : MonoBehaviour
{
    public Transform handTarget;
    public Level4TerrariumController terrarium;
    public float moveSpeed = 0.12f;
    public float stopDistance = 0.12f;

    private Vector3 startPosition;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (handTarget == null || terrarium == null || !terrarium.IsOpen())
            return;

        float distance = Vector3.Distance(transform.position, handTarget.position);

        if (distance > stopDistance)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                handTarget.position,
                moveSpeed * Time.deltaTime
            );
        }
    }

    public void ResetSpider()
    {
        transform.position = startPosition;
    }
}
