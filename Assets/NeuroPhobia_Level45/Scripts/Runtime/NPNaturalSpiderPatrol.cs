using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NeuroPhobia.Level45
{
    public sealed class NPNaturalSpiderPatrol : MonoBehaviour
    {
        [Header("Ruta")]
        public Transform[] waypoints;
        public float moveSpeed = 0.18f;
        public float turnSpeed = 5f;
        public float arrivalDistance = 0.10f;
        public float pauseAtPoint = 0.9f;
        public bool loop = true;

        [Header("Animación")]
        public Animator animator;
        public Transform visualRoot;
        public float animationSpeed = 0.35f;
        public bool useProceduralMotionWhenNoClip = true;

        [Header("Suelo")]
        public float rayHeight = 1.2f;
        public float rayDistance = 3.5f;
        public float groundOffset = 0.015f;
        public LayerMask groundMask = ~0;

        [Header("Seguridad")]
        public float maximumDistanceFromRoute = 12f;
        public float maximumVerticalDeviation = 1.5f;

        private int currentWaypoint;
        private float waitingUntil;
        private bool paused;
        private Vector3 safePosition;
        private float stableGroundY;
        private Vector3 visualBasePosition;
        private Quaternion visualBaseRotation;
        private bool animatorHasController;

        private void Awake()
        {
            PrepareObject();
        }

        private void Start()
        {
            safePosition = transform.position;
            stableGroundY = transform.position.y;
            SnapToGround();
            safePosition = transform.position;
        }

        private void Update()
        {
            EnsureVisible();

            if (PositionIsInvalid())
            {
                transform.position = safePosition;
            }

            if (paused ||
                waypoints == null ||
                waypoints.Length == 0 ||
                Time.time < waitingUntil)
            {
                UpdateAnimation(false);
                return;
            }

            Transform destinationTransform = waypoints[
                Mathf.Clamp(currentWaypoint, 0, waypoints.Length - 1)];

            if (destinationTransform == null)
            {
                AdvanceWaypoint();
                return;
            }

            Vector3 direction = destinationTransform.position - transform.position;
            direction.y = 0f;

            if (direction.magnitude <= arrivalDistance)
            {
                safePosition = transform.position;
                waitingUntil = Time.time + pauseAtPoint;
                AdvanceWaypoint();
                UpdateAnimation(false);
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(
                direction.normalized,
                Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                turnSpeed * Time.deltaTime);

            float angle = Quaternion.Angle(
                transform.rotation,
                desiredRotation);

            bool moving = angle < 35f;

            if (moving)
            {
                Vector3 nextPosition = Vector3.MoveTowards(
                    transform.position,
                    new Vector3(
                        destinationTransform.position.x,
                        transform.position.y,
                        destinationTransform.position.z),
                    moveSpeed * Time.deltaTime);

                transform.position = nextPosition;
                SnapToGround();
                safePosition = transform.position;
            }

            UpdateAnimation(moving);
        }

        public void PausePatrol()
        {
            paused = true;
            UpdateAnimation(false);
        }

        public void ResumePatrol()
        {
            paused = false;
        }

        private void PrepareObject()
        {
            Rigidbody rigidbodyComponent = GetComponent<Rigidbody>();
            if (rigidbodyComponent != null)
            {
                rigidbodyComponent.isKinematic = true;
                rigidbodyComponent.useGravity = false;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            if (visualRoot == null)
            {
                Renderer rendererComponent =
                    GetComponentInChildren<Renderer>(true);

                visualRoot = rendererComponent != null
                    ? rendererComponent.transform
                    : (transform.childCount > 0
                        ? transform.GetChild(0)
                        : transform);
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
                animatorHasController =
                    animator.runtimeAnimatorController != null;
                animator.enabled = animatorHasController;
                animator.speed = animatorHasController
                    ? animationSpeed
                    : 0f;
            }

            EnsureVisible();
        }

        private void EnsureVisible()
        {
            foreach (Renderer rendererComponent in
                     GetComponentsInChildren<Renderer>(true))
            {
                rendererComponent.enabled = true;
            }
        }

        private void AdvanceWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return;
            }

            currentWaypoint++;

            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = loop
                    ? 0
                    : waypoints.Length - 1;
            }
        }

        private void SnapToGround()
        {
            Vector3 origin = transform.position + Vector3.up * rayHeight;

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                rayDistance,
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

                if (hit.normal.y < 0.30f)
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

        private bool PositionIsInvalid()
        {
            Vector3 position = transform.position;

            if (float.IsNaN(position.x) ||
                float.IsNaN(position.y) ||
                float.IsNaN(position.z))
            {
                return true;
            }

            Vector3 routeCenter = GetRouteCenter();
            Vector3 flatOffset = position - routeCenter;
            flatOffset.y = 0f;

            return flatOffset.magnitude > maximumDistanceFromRoute ||
                   Mathf.Abs(position.y - stableGroundY) >
                   maximumVerticalDeviation;
        }

        private Vector3 GetRouteCenter()
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return safePosition;
            }

            Vector3 sum = Vector3.zero;
            int validCount = 0;

            foreach (Transform point in waypoints)
            {
                if (point == null)
                {
                    continue;
                }

                sum += point.position;
                validCount++;
            }

            return validCount > 0
                ? sum / validCount
                : safePosition;
        }

        private void UpdateAnimation(bool moving)
        {
            if (animatorHasController && animator != null)
            {
                animator.speed = moving ? animationSpeed : 0f;
                return;
            }

            if (!useProceduralMotionWhenNoClip ||
                visualRoot == null)
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

            float phase = Time.time * 7f;
            float bob = Mathf.Sin(phase) * 0.006f;
            float roll = Mathf.Sin(phase * 0.5f) * 1.2f;

            visualRoot.localPosition =
                visualBasePosition + Vector3.up * bob;

            visualRoot.localRotation =
                visualBaseRotation *
                Quaternion.Euler(0f, 0f, roll);
        }
    }
}
