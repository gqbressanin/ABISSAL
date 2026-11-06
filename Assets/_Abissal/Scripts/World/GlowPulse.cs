using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abissal.World
{
    /// <summary>Faz uma luz pulsar devagar, como um brilho vivo.</summary>
    [RequireComponent(typeof(Light2D))]
    public class GlowPulse : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float minIntensity = 0.8f;
        [SerializeField, Min(0f)] private float maxIntensity = 1.6f;
        [SerializeField, Min(0f)] private float speed = 2f;

        private Light2D glow;

        private void Awake()
        {
            glow = GetComponent<Light2D>();
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * speed) * 0.5f + 0.5f;
            glow.intensity = Mathf.Lerp(minIntensity, maxIntensity, wave);
        }
    }
}
