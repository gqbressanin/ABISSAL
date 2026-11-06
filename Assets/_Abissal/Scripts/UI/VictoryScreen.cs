using Abissal.Core;
using Abissal.Expedition;
using Abissal.Systems;
using UnityEngine;

namespace Abissal.UI
{
    /// <summary>Mostra o resultado da expedição e reinicia ao pressionar ESPAÇO.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class VictoryScreen : MonoBehaviour
    {
        [SerializeField] private SampleTracker sampleTracker;
        [SerializeField] private DepthTracker depthTracker;

        [Header("Textos")]
        [SerializeField] private PixelText samplesLine;
        [SerializeField] private PixelText completionLine;
        [SerializeField] private PixelText depthLine;
        [SerializeField] private PixelText timeLine;
        [SerializeField] private PixelText perfectLine;
        [SerializeField] private CanvasGroup promptGroup;

        [SerializeField, Min(0.1f)] private float fadeSeconds = 1f;

        private CanvasGroup group;
        private float elapsed;
        private bool isFilled;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Victory)
            {
                return;
            }

            if (!isFilled)
            {
                FillResults();
                isFilled = true;
            }

            elapsed += Time.deltaTime;
            group.alpha = Mathf.Clamp01(elapsed / fadeSeconds);
            promptGroup.alpha = Mathf.Lerp(0.35f, 1f, Mathf.Sin(Time.time * 2.4f) * 0.5f + 0.5f);

            if (group.alpha >= 1f && Input.GetKeyDown(KeyCode.Space))
            {
                GameManager.Instance.RestartExpedition();
            }
        }

        private void FillResults()
        {
            int seconds = Mathf.FloorToInt(GameManager.Instance.ExpeditionSeconds);
            int percent = sampleTracker.CompletionPercent;

            samplesLine.SetText($"Amostras secundárias: {sampleTracker.SecondaryCollected}/{sampleTracker.SecondaryTotal}");
            completionLine.SetText($"Conclusão: {percent}%");
            depthLine.SetText($"Profundidade máxima: {depthTracker.MaxDepth:F0} m");
            timeLine.SetText($"Tempo: {seconds / 60:00}:{seconds % 60:00}");
            perfectLine.gameObject.SetActive(percent == 100);
        }
    }
}
