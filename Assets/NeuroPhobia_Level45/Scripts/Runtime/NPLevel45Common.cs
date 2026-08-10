using System.Linq;
using TMPro;
using UnityEngine;

namespace NeuroPhobia.Level45
{
    public static class NPLevel45Camera
    {
        public static Camera Find()
        {
            Camera tagged = Camera.main;
            if (tagged != null && tagged.enabled && tagged.gameObject.activeInHierarchy)
            {
                return tagged;
            }

            return Object.FindObjectsByType<Camera>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .FirstOrDefault(cameraComponent =>
                    cameraComponent != null &&
                    cameraComponent.enabled &&
                    cameraComponent.gameObject.activeInHierarchy);
        }

        public static Vector3 TargetCenter(Transform target)
        {
            if (target == null)
            {
                return Vector3.zero;
            }

            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                return target.position;
            }

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return bounds.center;
        }

        public static bool IsLookingAt(
            Camera cameraComponent,
            Transform target,
            float viewportRadius)
        {
            if (cameraComponent == null || target == null)
            {
                return false;
            }

            Vector3 viewportPoint = cameraComponent.WorldToViewportPoint(
                TargetCenter(target));

            if (viewportPoint.z <= 0f)
            {
                return false;
            }

            Vector2 offset = new Vector2(
                viewportPoint.x - 0.5f,
                viewportPoint.y - 0.5f);

            return offset.magnitude <= viewportRadius;
        }
    }

    [RequireComponent(typeof(Canvas))]
    public sealed class NPFixedCanvasCameraBinder : MonoBehaviour
    {
        private Canvas targetCanvas;

        private void Awake()
        {
            targetCanvas = GetComponent<Canvas>();
            Bind();
        }

        private void LateUpdate()
        {
            Bind();
        }

        private void Bind()
        {
            if (targetCanvas == null)
            {
                return;
            }

            if (targetCanvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                return;
            }

            if (targetCanvas.worldCamera == null ||
                !targetCanvas.worldCamera.gameObject.activeInHierarchy)
            {
                targetCanvas.worldCamera = NPLevel45Camera.Find();
            }
        }
    }

    public static class NPLevel45Text
    {
        public static void Configure(TMP_Text textComponent)
        {
            if (textComponent == null)
            {
                return;
            }

            textComponent.textWrappingMode = TextWrappingModes.Normal;
            textComponent.raycastTarget = false;
        }
    }
}
