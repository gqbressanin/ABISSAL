using UnityEngine;

namespace Abissal.World
{
    /// <summary>Faz um peixe nadar entre dois limites horizontais, oscilando levemente.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AmbientSwimmer : MonoBehaviour
    {
        [SerializeField] private float minX = -18f;
        [SerializeField] private float maxX = 18f;
        [SerializeField, Min(0f)] private float speed = 1.5f;
        [SerializeField, Min(0f)] private float bobHeight = 0.2f;
        [SerializeField, Min(0f)] private float bobSpeed = 1.5f;

        private SpriteRenderer spriteRenderer;
        private float baseY;
        private float bobOffset;
        private int direction = 1;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            baseY = transform.position.y;
            bobOffset = Random.value * Mathf.PI * 2f;
            direction = Random.value < 0.5f ? -1 : 1;
        }

        private void Update()
        {
            Vector3 position = transform.position;
            position.x += direction * speed * Time.deltaTime;
            position.y = baseY + Mathf.Sin(Time.time * bobSpeed + bobOffset) * bobHeight;

            if (position.x > maxX)
            {
                direction = -1;
            }
            else if (position.x < minX)
            {
                direction = 1;
            }

            transform.position = position;
            spriteRenderer.flipX = direction < 0;
        }
    }
}
