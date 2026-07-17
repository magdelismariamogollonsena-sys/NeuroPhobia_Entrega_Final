using UnityEngine;

public class Level3AvoidanceMonitor : MonoBehaviour
{
    public Transform playerRoot;
    public Transform spiderTarget;
    public Level3HUD hud;
    public float sampleInterval = 0.5f;
    public float retreatThreshold = 0.75f;
    public float suddenHeadTurnDegrees = 65f;
    private float timer;
    private float lastDistance;
    private Quaternion lastRotation;

    private void Start()
    {
        if (playerRoot == null)
        {
            GameObject xrOrigin = GameObject.Find("XR Origin (XR Rig)");
            if (xrOrigin != null) playerRoot = xrOrigin.transform;
        }
        if (Camera.main != null) lastRotation = Camera.main.transform.rotation;
        if (playerRoot != null && spiderTarget != null) lastDistance = Vector3.Distance(playerRoot.position, spiderTarget.position);
    }

    private void Update()
    {
        if (playerRoot == null || spiderTarget == null || Camera.main == null) return;
        timer += Time.deltaTime;
        if (timer < sampleInterval) return;
        timer = 0f;
        float currentDistance = Vector3.Distance(playerRoot.position, spiderTarget.position);
        float headTurn = Quaternion.Angle(lastRotation, Camera.main.transform.rotation);
        if (currentDistance - lastDistance >= retreatThreshold || headTurn >= suddenHeadTurnDegrees)
        {
            if (hud != null) hud.ShowWarning("Haz una pausa. Respira lentamente antes de continuar.", 4f);
        }
        lastDistance = currentDistance;
        lastRotation = Camera.main.transform.rotation;
    }
}
