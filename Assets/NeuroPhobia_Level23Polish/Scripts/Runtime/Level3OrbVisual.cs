using UnityEngine;

public sealed class Level3OrbVisual : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 35f;
    [SerializeField] private float pulseSpeed = 1.4f;
    [SerializeField] private float pulseAmount = 0.12f;
    private Vector3 baseScale;

    private void OnEnable() => baseScale = transform.localScale;

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        float scale = 1f + Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) * pulseAmount;
        transform.localScale = baseScale * scale;
    }
}
