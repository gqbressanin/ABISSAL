using UnityEngine;

namespace Abissal.World
{
    /// <summary>Troca os sprites em sequência. É a animação usada por peixes, algas e pelo mergulhador.</summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;
        [Tooltip("Começa em um quadro aleatório para objetos iguais não ficarem sincronizados.")]
        [SerializeField] private bool randomStart = true;

        private SpriteRenderer spriteRenderer;
        private float elapsed;

        public float FramesPerSecond
        {
            get => framesPerSecond;
            set => framesPerSecond = Mathf.Max(0.1f, value);
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (randomStart)
            {
                elapsed = Random.value * 10f;
            }
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            elapsed += Time.deltaTime * framesPerSecond;
            spriteRenderer.sprite = frames[(int)elapsed % frames.Length];
        }
    }
}
