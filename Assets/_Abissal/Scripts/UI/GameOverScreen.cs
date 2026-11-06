using Abissal.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Abissal.UI
{
    /// <summary>Mostra o motivo da derrota e reinicia a expedição ao pressionar ESPAÇO.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private Image reasonImage;
        [SerializeField] private Sprite outOfOxygenSprite;
        [SerializeField] private Sprite outOfLivesSprite;
        [SerializeField] private CanvasGroup promptGroup;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 0.8f;

        private CanvasGroup group;
        private float elapsed;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.GameOver)
            {
                return;
            }

            reasonImage.sprite = GameManager.Instance.DefeatReason == GameOverReason.OutOfOxygen
                ? outOfOxygenSprite
                : outOfLivesSprite;

            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeSeconds);
            promptGroup.alpha = Mathf.Lerp(0.35f, 1f, Mathf.Sin(Time.time * 2.4f) * 0.5f + 0.5f);

            // A espera evita reiniciar sem querer quem ainda estava apertando teclas.
            if (group.alpha >= 1f && Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.RestartExpedition();
            }
        }
    }
}
