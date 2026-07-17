using UnityEngine;

public class Level2ExclusionZone : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerRoot;
    public Transform protectedTarget;
    public Level2HUD hud;

    [Header("Distancia segura")]
    public float safeDistance = 8f;
    public float returnExtraDistance = 0.5f;
    public float checkInterval = 0.1f;

    private float timer;

    private void Start()
    {
        if (playerRoot == null)
        {
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin != null)
            {
                playerRoot = xrOrigin.transform;
            }
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        Renderer r = GetComponent<Renderer>();
        if (r != null)
        {
            r.enabled = false;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        if (playerRoot == null || protectedTarget == null) return;

        Vector3 flatPlayer = new Vector3(playerRoot.position.x, 0f, playerRoot.position.z);
        Vector3 flatTarget = new Vector3(protectedTarget.position.x, 0f, protectedTarget.position.z);

        float distance = Vector3.Distance(flatPlayer, flatTarget);

        if (distance < safeDistance)
        {
            PushPlayerBack(flatPlayer, flatTarget);
        }
    }

    private void PushPlayerBack(Vector3 flatPlayer, Vector3 flatTarget)
    {
        Vector3 direction = (flatPlayer - flatTarget).normalized;

        if (direction.sqrMagnitude < 0.01f)
        {
            direction = Vector3.back;
        }

        Vector3 safeFlatPosition = flatTarget + direction * (safeDistance + returnExtraDistance);
        playerRoot.position = new Vector3(safeFlatPosition.x, playerRoot.position.y, safeFlatPosition.z);

        if (hud != null)
        {
            hud.ShowWarning("Manten una distancia segura. Observa desde fuera de la zona.", 2f);
        }
    }
}
