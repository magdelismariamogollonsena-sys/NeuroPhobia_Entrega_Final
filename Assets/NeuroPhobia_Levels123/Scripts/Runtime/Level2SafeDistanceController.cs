using UnityEngine;

public class Level2SafeDistanceController : MonoBehaviour
{
    public Transform playerRoot;
    public Transform spiderTarget;
    public Level2HUD hud;
    public float safeDistance = 8f;
    public float returnMargin = 0.35f;
    public float checkInterval = 0.08f;
    private float timer;

    private void Start() => ResolvePlayer();

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;
        if (playerRoot == null || !playerRoot.gameObject.activeInHierarchy) ResolvePlayer();
        if (playerRoot == null || spiderTarget == null) return;

        Vector3 playerFlat = new Vector3(playerRoot.position.x, 0f, playerRoot.position.z);
        Vector3 spiderFlat = new Vector3(spiderTarget.position.x, 0f, spiderTarget.position.z);
        float distance = Vector3.Distance(playerFlat, spiderFlat);
        if (distance >= safeDistance) return;

        Vector3 direction = (playerFlat - spiderFlat).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.back;
        Vector3 target = spiderFlat + direction * (safeDistance + returnMargin);
        MovePlayerTo(new Vector3(target.x, playerRoot.position.y, target.z));
        if (hud != null) hud.ShowWarning("Mantén una distancia segura de 8 metros.", 2f);
    }

    private void ResolvePlayer()
    {
        GameObject desktop = GameObject.Find("DesktopTestRig");
        if (desktop != null && desktop.activeInHierarchy) { playerRoot = desktop.transform; return; }
        GameObject xr = GameObject.Find("XR Origin (XR Rig)");
        if (xr != null) { playerRoot = xr.transform; return; }
        CharacterController cc = FindFirstObjectByType<CharacterController>();
        if (cc != null) playerRoot = cc.transform;
    }

    private void MovePlayerTo(Vector3 position)
    {
        CharacterController cc = playerRoot.GetComponent<CharacterController>();
        if (cc == null) cc = playerRoot.GetComponentInChildren<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            playerRoot.position = position;
            cc.enabled = true;
        }
        else playerRoot.position = position;
    }
}
