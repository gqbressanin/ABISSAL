using Abissal.Core;
using Abissal.Expedition;
using Abissal.Player;
using Abissal.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Abissal.UI
{
    /// <summary>Mostra vidas e oxigênio. Apenas lê os sistemas, sem conter regras do jogo.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class Hud : MonoBehaviour
    {
        [SerializeField] private OxygenSystem oxygen;
        [SerializeField] private PlayerHealth health;
        [SerializeField] private Image oxygenFill;
        [SerializeField] private Image[] hearts;
        [SerializeField] private Sprite heartFull;
        [SerializeField] private Sprite heartEmpty;

        [Header("Amostras")]
        [SerializeField] private SampleTracker sampleTracker;
        [SerializeField] private SampleCollector sampleCollector;
        [SerializeField] private Text samplesLabel;
        [SerializeField] private CanvasGroup collectPrompt;
        [SerializeField] private Color normalTextColor = Color.white;
        [SerializeField] private Color collectedTextColor = new Color(1f, 0.85f, 0.35f);
        [SerializeField, Min(0.1f)] private float collectedFlashSeconds = 1.2f;

        [Header("Alerta de oxigênio")]
        [SerializeField] private Color normalColor = new Color(0.45f, 0.9f, 1f);
        [SerializeField] private Color warningColor = new Color(1f, 0.3f, 0.25f);
        [SerializeField, Min(0f)] private float warningPulseSpeed = 8f;

        private CanvasGroup group;
        private float lastCollectedTime = float.NegativeInfinity;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        private void Start()
        {
            health.Hit += RefreshHearts;
            sampleTracker.SampleCollected += OnSampleCollected;
            RefreshHearts();
        }

        private void OnDestroy()
        {
            health.Hit -= RefreshHearts;
            sampleTracker.SampleCollected -= OnSampleCollected;
        }

        private void OnSampleCollected(SampleItem sample)
        {
            lastCollectedTime = Time.time;
        }

        private void Update()
        {
            group.alpha = GameManager.Instance.State == GameState.Playing ? 1f : 0f;
            oxygenFill.fillAmount = oxygen.Normalized;
            oxygenFill.color = CurrentOxygenColor();
            RefreshSamples();
        }

        private void RefreshSamples()
        {
            string mainStatus = sampleTracker.HasMainSample ? "coletada" : "pendente";
            samplesLabel.text = $"Amostras: {sampleTracker.SecondaryCollected}/{sampleTracker.SecondaryTotal}\n"
                + $"Principal: {mainStatus}\n"
                + $"Conclusão: {sampleTracker.CompletionPercent}%";

            bool justCollected = Time.time < lastCollectedTime + collectedFlashSeconds;
            samplesLabel.color = justCollected ? collectedTextColor : normalTextColor;
            collectPrompt.alpha = sampleCollector.NearbySample != null ? 1f : 0f;
        }

        private Color CurrentOxygenColor()
        {
            if (!oxygen.IsLow)
            {
                return normalColor;
            }

            float pulse = Mathf.Sin(Time.time * warningPulseSpeed) * 0.5f + 0.5f;
            return Color.Lerp(warningColor, Color.white, pulse * 0.5f);
        }

        private void RefreshHearts()
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                hearts[i].sprite = i < health.Lives ? heartFull : heartEmpty;
            }
        }
    }
}
