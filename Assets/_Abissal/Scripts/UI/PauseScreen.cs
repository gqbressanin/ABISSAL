using Abissal.Core;
using UnityEngine;

namespace Abissal.UI
{
    /// <summary>Mostra a tela de pausa. Quem pausa de fato é o GameManager.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class PauseScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup promptGroup;

        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        private void Update()
        {
            bool isPaused = GameManager.Instance.State == GameState.Paused;
            group.alpha = isPaused ? 1f : 0f;

            if (isPaused)
            {
                // O tempo normal está parado, então a pulsação usa o tempo real.
                promptGroup.alpha = Mathf.Lerp(0.35f, 1f, Mathf.Sin(Time.unscaledTime * 2.4f) * 0.5f + 0.5f);
            }
        }
    }
}
