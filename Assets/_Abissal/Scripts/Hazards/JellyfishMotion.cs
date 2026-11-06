using UnityEngine;

namespace Abissal.Hazards
{
    /// <summary>Movimento de água-viva: sobe e desce devagar enquanto deriva para os lados.</summary>
    public class JellyfishMotion : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float verticalRange = 1.2f;
        [SerializeField, Min(0f)] private float horizontalRange = 0.8f;
        [SerializeField, Min(0f)] private float speed = 0.5f;

        private Vector3 origin;
        private float offset;

        private void Awake()
        {
            origin = transform.position;
            offset = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            float time = Time.time * speed + offset;
            transform.position = origin + new Vector3(
                Mathf.Sin(time * 0.6f) * horizontalRange,
                Mathf.Sin(time) * verticalRange,
                0f);
        }
    }
}
