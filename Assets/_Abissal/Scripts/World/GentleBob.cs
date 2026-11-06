using UnityEngine;

namespace Abissal.World
{
    /// <summary>Balanço suave, usado no barco parado na superfície.</summary>
    public class GentleBob : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float height = 0.08f;
        [SerializeField, Min(0f)] private float tiltDegrees = 1.2f;
        [SerializeField, Min(0f)] private float speed = 1.1f;

        private Vector3 basePosition;

        private void Awake()
        {
            basePosition = transform.position;
        }

        private void Update()
        {
            float wave = Mathf.Sin(Time.time * speed);
            transform.position = basePosition + Vector3.up * (wave * height);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Cos(Time.time * speed * 0.8f) * tiltDegrees);
        }
    }
}
