using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// Patrulla segura de la araña del Nivel 3.
/// Ignora los colliders de la propia araña al buscar el suelo,
/// conserva la ruta alrededor de la posición colocada en la escena
/// y evita que el modelo se desplace fuera del sótano.
/// </summary>
public class Level3SpiderPatrolV2 : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float moveSpeed = 0.13f;
    public float reducedSpeedMultiplier = 0.5f;
    public float turnSpeed = 4.5f;
    public Animator animator;
    public float animatorSpeed = 0.28f;
    public Transform visualRoot;
    public float endpointPauseMin = 0.8f;
    public float endpointPauseMax = 1.8f;
    public float groundRayHeight = 1.5f;
    public float groundOffset = 0.015f;
    public LayerMask groundMask = ~0;

    [Header("Protección de ruta")]
    public float patrolHalfDistance = 1.8f;
    public float maxRouteDistanceFromSpawn = 6f;
    public float maxVerticalDeviation = 1.2f;

    private Transform target;
    private bool paused;
    private bool reduced;
    private bool waiting;
    private Vector3 lastPosition;
    private Vector3 visualBasePosition;
    private Quaternion visualBaseRotation;
    private bool hasAnimatorMotion;
    private Vector3 initialSpawn;
    private Quaternion initialRotation;
    private float stableGroundY;

    private void OnEnable()
    {
        GameStateManager.OnStateChanged += HandleState;
    }

    private void OnDisable()
    {
        GameStateManager.OnStateChanged -= HandleState;
    }

    private void Start()
    {
        initialSpawn = transform.position;
        initialRotation = transform.rotation;
        stableGroundY = initialSpawn.y;

        PreparePhysics();
        PrepareVisuals();
        ValidateOrRecenterRoute();

        target = pointB != null ? pointB : pointA;
        lastPosition = transform.position;

        SnapToGroundSafe();
    }

    private void Update()
    {
        EnsureVisible();

        if (RoutePositionIsInvalid())
        {
            ResetToSafeRoutePosition();
        }

        if (paused || waiting || pointA == null || pointB == null || target == null)
        {
            SetAnimatorMoving(false, 0f);
            ApplyFallbackLegMotion(false, 0f);
            return;
        }

        float speed = moveSpeed * (reduced ? reducedSpeedMultiplier : 1f);
        Vector3 destination = target.position;
        Vector3 flatDirection = destination - transform.position;
        flatDirection.y = 0f;
        float distance = flatDirection.magnitude;

        if (distance <= 0.055f)
        {
            target = target == pointA ? pointB : pointA;
            StartCoroutine(WaitAtEndpoint(Random.Range(endpointPauseMin, endpointPauseMax)));
            return;
        }

        if (flatDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRotation =
                Quaternion.LookRotation(flatDirection.normalized, Vector3.up);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                desiredRotation,
                turnSpeed * 90f * Time.deltaTime);

            float angle = Quaternion.Angle(transform.rotation, desiredRotation);

            if (angle < 32f)
            {
                Vector3 next = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    speed * Time.deltaTime);

                next.y = transform.position.y;
                transform.position = next;
                SnapToGroundSafe();
            }
        }

        float actualSpeed =
            Vector3.Distance(lastPosition, transform.position) /
            Mathf.Max(Time.deltaTime, 0.0001f);

        lastPosition = transform.position;
        bool moving = actualSpeed > 0.005f;

        SetAnimatorMoving(
            moving,
            actualSpeed / Mathf.Max(moveSpeed, 0.001f));

        ApplyFallbackLegMotion(moving, actualSpeed);
    }

    private void LateUpdate()
    {
        EnsureVisible();
    }

    private void PreparePhysics()
    {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }

    private void PrepareVisuals()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (visualRoot == null)
        {
            Transform named = transform.Find("Visual_WolfSpider");
            visualRoot = named != null
                ? named
                : (transform.childCount > 0 ? transform.GetChild(0) : null);
        }

        if (visualRoot != null)
        {
            visualBasePosition = visualRoot.localPosition;
            visualBaseRotation = visualRoot.localRotation;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            hasAnimatorMotion = animator.runtimeAnimatorController != null;
            animator.enabled = hasAnimatorMotion;
            animator.speed = hasAnimatorMotion ? animatorSpeed : 0f;
        }

        EnsureVisible();
    }

    private void ValidateOrRecenterRoute()
    {
        bool missingRoute = pointA == null || pointB == null;

        bool routeTooFar = !missingRoute &&
            (Vector3.Distance(pointA.position, initialSpawn) > maxRouteDistanceFromSpawn ||
             Vector3.Distance(pointB.position, initialSpawn) > maxRouteDistanceFromSpawn);

        if (!missingRoute && !routeTooFar)
        {
            return;
        }

        Transform routeParent = transform.parent != null
            ? transform.parent
            : null;

        if (pointA == null)
        {
            GameObject pointObject = new GameObject("Patrol_A");
            pointA = pointObject.transform;
            pointA.SetParent(routeParent, true);
        }

        if (pointB == null)
        {
            GameObject pointObject = new GameObject("Patrol_B");
            pointB = pointObject.transform;
            pointB.SetParent(routeParent, true);
        }

        Vector3 routeAxis = transform.right;
        routeAxis.y = 0f;
        if (routeAxis.sqrMagnitude < 0.001f)
        {
            routeAxis = Vector3.right;
        }
        routeAxis.Normalize();

        pointA.position = initialSpawn - routeAxis * patrolHalfDistance;
        pointB.position = initialSpawn + routeAxis * patrolHalfDistance;
    }

    private void SnapToGroundSafe()
    {
        Vector3 origin = transform.position + Vector3.up * groundRayHeight;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            Vector3.down,
            groundRayHeight * 3f,
            groundMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits.OrderBy(candidate => candidate.distance))
        {
            if (hit.collider == null)
            {
                continue;
            }

            Transform hitTransform = hit.collider.transform;

            if (hitTransform == transform ||
                hitTransform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.normal.y < 0.20f)
            {
                continue;
            }

            Vector3 position = transform.position;
            position.y = hit.point.y + groundOffset;
            transform.position = position;
            stableGroundY = position.y;
            return;
        }

        Vector3 fallback = transform.position;
        fallback.y = stableGroundY;
        transform.position = fallback;
    }

    private bool RoutePositionIsInvalid()
    {
        if (float.IsNaN(transform.position.x) ||
            float.IsNaN(transform.position.y) ||
            float.IsNaN(transform.position.z))
        {
            return true;
        }

        Vector3 routeCenter = GetRouteCenter();
        Vector3 flatOffset = transform.position - routeCenter;
        flatOffset.y = 0f;

        if (flatOffset.magnitude > maxRouteDistanceFromSpawn)
        {
            return true;
        }

        return Mathf.Abs(transform.position.y - stableGroundY) >
               maxVerticalDeviation;
    }

    private Vector3 GetRouteCenter()
    {
        if (pointA != null && pointB != null)
        {
            return (pointA.position + pointB.position) * 0.5f;
        }

        return initialSpawn;
    }

    private void ResetToSafeRoutePosition()
    {
        Vector3 resetPosition = GetRouteCenter();
        resetPosition.y = stableGroundY;

        transform.SetPositionAndRotation(
            resetPosition,
            initialRotation);

        lastPosition = resetPosition;
        target = pointB != null ? pointB : pointA;
    }

    private void EnsureVisible()
    {
        foreach (Renderer rendererComponent in
                 GetComponentsInChildren<Renderer>(true))
        {
            rendererComponent.enabled = true;
        }
    }

    private void SetAnimatorMoving(bool moving, float speedRatio)
    {
        if (!hasAnimatorMotion || animator == null)
        {
            return;
        }

        animator.speed = moving
            ? animatorSpeed *
              Mathf.Clamp(speedRatio, 0.55f, 1.25f) *
              (reduced ? reducedSpeedMultiplier : 1f)
            : 0f;
    }

    private void ApplyFallbackLegMotion(bool moving, float actualSpeed)
    {
        if (hasAnimatorMotion || visualRoot == null)
        {
            return;
        }

        if (!moving)
        {
            visualRoot.localPosition = Vector3.Lerp(
                visualRoot.localPosition,
                visualBasePosition,
                Time.deltaTime * 5f);

            visualRoot.localRotation = Quaternion.Slerp(
                visualRoot.localRotation,
                visualBaseRotation,
                Time.deltaTime * 5f);

            return;
        }

        float phase = Time.time * (5.5f + actualSpeed * 12f);
        float bob = Mathf.Sin(phase) * 0.006f;
        float roll = Mathf.Sin(phase * 0.5f) * 1.1f;

        visualRoot.localPosition =
            visualBasePosition + Vector3.up * bob;

        visualRoot.localRotation =
            visualBaseRotation * Quaternion.Euler(0f, 0f, roll);
    }

    private IEnumerator WaitAtEndpoint(float seconds)
    {
        waiting = true;
        SetAnimatorMoving(false, 0f);
        yield return new WaitForSeconds(seconds);
        waiting = false;
    }

    public void SetReducedSpeed(bool value)
    {
        reduced = value;
    }

    public void PauseMovement()
    {
        paused = true;
        SetAnimatorMoving(false, 0f);
    }

    public void ResumeMovement()
    {
        paused = false;
    }

    public void PauseFor(float seconds)
    {
        StartCoroutine(PauseRoutine(seconds));
    }

    private IEnumerator PauseRoutine(float seconds)
    {
        PauseMovement();
        yield return new WaitForSeconds(seconds);

        if (GameStateManager.CurrentState == GameState.PLAYING)
        {
            ResumeMovement();
        }
    }

    private void HandleState(GameState state)
    {
        if (state == GameState.BREATHING ||
            state == GameState.PAUSED)
        {
            PauseMovement();
        }
        else if (state == GameState.PLAYING)
        {
            ResumeMovement();
        }
    }
}
