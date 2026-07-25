using System.Linq;
using UnityEngine;

/// <summary>
/// Guardia externa para impedir que la araña del Nivel 3 sea desactivada
/// o enviada fuera de su zona por otro componente.
/// </summary>
public sealed class Level3SpiderVisibilityGuard : MonoBehaviour
{
    [SerializeField] private GameObject spider;
    [SerializeField] private Vector3 safePosition;
    [SerializeField] private Quaternion safeRotation = Quaternion.identity;
    [SerializeField] private float maximumDistance = 8f;
    [SerializeField] private float maximumVerticalDifference = 1.5f;

    private void Awake()
    {
        RestoreSpider();
    }

    private void LateUpdate()
    {
        RestoreSpider();
    }

    public void Configure(
        GameObject spiderObject,
        Vector3 position,
        Quaternion rotation)
    {
        spider = spiderObject;
        safePosition = position;
        safeRotation = rotation;
    }

    private void RestoreSpider()
    {
        if (spider == null)
        {
            Transform match = FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .FirstOrDefault(candidate =>
                    candidate != null &&
                    (candidate.name == "NP_WolfSpider_Nivel3_Patrol" ||
                     candidate.name.ToLowerInvariant().Contains("wolfspider_nivel3")));

            if (match != null)
            {
                spider = match.gameObject;
            }
        }

        if (spider == null)
        {
            return;
        }

        if (!spider.activeSelf)
        {
            spider.SetActive(true);
        }

        Vector3 offset = spider.transform.position - safePosition;
        Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);

        bool invalidPosition =
            float.IsNaN(spider.transform.position.x) ||
            float.IsNaN(spider.transform.position.y) ||
            float.IsNaN(spider.transform.position.z) ||
            flatOffset.magnitude > maximumDistance ||
            Mathf.Abs(offset.y) > maximumVerticalDifference;

        if (invalidPosition)
        {
            spider.transform.SetPositionAndRotation(
                safePosition,
                safeRotation);
        }

        foreach (Renderer rendererComponent in
                 spider.GetComponentsInChildren<Renderer>(true))
        {
            rendererComponent.enabled = true;
        }

        Animator animator =
            spider.GetComponentInChildren<Animator>(true);

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.enabled = true;
        }

        Rigidbody body = spider.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
