using UnityEngine;

namespace Abissal.World
{
    /// <summary>Faz um raio de luz pulsar e deslizar um pouco, simulando a luz atravessando a água.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LightRaySway : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float minAlpha = 0.25f;
        [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.8f;
        [SerializeField, Min(0f)] private float speed = 0.5f;
        [SerializeField, Min(0f)] private float slideDistance = 0.6f;

        private SpriteRenderer spriteRenderer;
        private Vector3 basePosition;
        private float offset;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            basePosition = transform.position;
            offset = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * speed + offset);

            Color color = spriteRenderer.color;
            color.a = Mathf.Lerp(minAlpha, maxAlpha, wave * 0.5f + 0.5f);
            spriteRenderer.color = color;

            transform.position = basePosition + Vector3.right * (wave * slideDistance);
        }
    }
}
