using Abissal.Player;
using UnityEngine;

namespace Abissal.World
{
    /// <summary>
    /// Área secreta coberta por uma camada de rocha falsa.
    /// A camada fica transparente enquanto o mergulhador está dentro da área.
    /// Precisa de um Collider2D marcado como Trigger.
    /// </summary>
    public class HiddenAreaReveal : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer coverLayer;
        [SerializeField, Range(0f, 1f)] private float revealedAlpha = 0.12f;
        [SerializeField, Min(0.1f)] private float fadeSpeed = 3f;

        private bool isDiverInside;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (IsDiver(other))
            {
                isDiverInside = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (IsDiver(other))
            {
                isDiverInside = false;
            }
        }

        private void Update()
        {
            Color color = coverLayer.color;
            color.a = Mathf.MoveTowards(color.a, isDiverInside ? revealedAlpha : 1f, fadeSpeed * Time.deltaTime);
            coverLayer.color = color;
        }

        private static bool IsDiver(Collider2D other)
        {
            return other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out DiverController _);
        }
    }
}
