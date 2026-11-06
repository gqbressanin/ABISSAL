using Abissal.Core;
using Abissal.Expedition;
using UnityEngine;
using UnityEngine.UI;

namespace Abissal.UI
{
    /// <summary>Mostra o objetivo no início da expedição e avisa quando a amostra principal é coletada.</summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ObjectiveBanner : MonoBehaviour
    {
        private const float PixelScale = 4f;

        [SerializeField] private SampleTracker sampleTracker;
        [SerializeField] private Image messageImage;
        [SerializeField] private Sprite startMessage;
        [SerializeField] private Sprite returnMessage;
        [SerializeField] private SurfaceZone surfaceZone;
        [SerializeField] private Sprite missingSampleMessage;
        [SerializeField, Min(0.1f)] private float visibleSeconds = 4f;
        [SerializeField, Min(0.1f)] private float fadeSeconds = 0.6f;

        private CanvasGroup group;
        private float hideTime = float.NegativeInfinity;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
        }

        private void Start()
        {
            GameManager.Instance.StateChanged += OnStateChanged;
            sampleTracker.SampleCollected += OnSampleCollected;
            surfaceZone.ReachedWithoutMainSample += OnReachedWithoutMainSample;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= OnStateChanged;
            }

            sampleTracker.SampleCollected -= OnSampleCollected;
            surfaceZone.ReachedWithoutMainSample -= OnReachedWithoutMainSample;
        }

        private void OnReachedWithoutMainSample()
        {
            Show(missingSampleMessage);
        }

        private void Update()
        {
            float target = Time.time < hideTime && GameManager.Instance.State == GameState.Playing ? 1f : 0f;
            group.alpha = Mathf.MoveTowards(group.alpha, target, Time.deltaTime / fadeSeconds);
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Playing)
            {
                Show(startMessage);
            }
        }

        private void OnSampleCollected(SampleItem sample)
        {
            if (sample.IsMainSample)
            {
                Show(returnMessage);
            }
        }

        private void Show(Sprite message)
        {
            messageImage.sprite = message;
            messageImage.rectTransform.sizeDelta = message.rect.size * PixelScale;
            hideTime = Time.time + visibleSeconds;
        }
    }
}
