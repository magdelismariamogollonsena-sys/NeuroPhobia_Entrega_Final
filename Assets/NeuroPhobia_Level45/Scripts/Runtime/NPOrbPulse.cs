using UnityEngine;

namespace NeuroPhobia.Level45
{
    public sealed class NPOrbPulse : MonoBehaviour
    {
        public float rotationSpeed = 45f;
        public float pulseSpeed = 2.2f;
        public float pulseAmount = 0.12f;

        private Vector3 baseScale;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        private void Update()
        {
            transform.Rotate(
                Vector3.up,
                rotationSpeed * Time.deltaTime,
                Space.World);

            float pulse = 1f +
                Mathf.Sin(Time.time * pulseSpeed) *
                pulseAmount;

            transform.localScale = baseScale * pulse;
        }
    }
}
