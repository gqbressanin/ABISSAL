using System.Collections;
using Abissal.Core;
using Abissal.World;
using UnityEngine;

namespace Abissal.UI
{
    /// <summary>
    /// Tela inicial: mostra o título sobre o oceano e espera o ESPAÇO.
    /// A movimentação do cenário vem dos próprios objetos da cena.
    /// </summary>
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup titleGroup;
        [SerializeField] private CanvasGroup promptGroup;
        [SerializeField] private DiveIntro diveIntro;
        [SerializeField, Min(0.1f)] private float fadeDuration = 0.9f;
        [SerializeField, Min(0f)] private float promptPulseSpeed = 2.4f;

        private bool started;

        private void Start()
        {
            if (GameManager.SkipTitleOnLoad)
            {
                GameManager.SkipTitleOnLoad = false;
                titleGroup.alpha = 0f;
                promptGroup.alpha = 0f;
                started = true;
                diveIntro.Play(GameManager.Instance.BeginExpedition);
            }
        }

        private void Update()
        {
            if (started || GameManager.Instance.State != GameState.Title)
            {
                return;
            }

            promptGroup.alpha = Mathf.Lerp(0.35f, 1f, Mathf.Sin(Time.time * promptPulseSpeed) * 0.5f + 0.5f);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartDive();
            }
        }

        private void StartDive()
        {
            started = true;
            StartCoroutine(FadeOut());
            diveIntro.Play(GameManager.Instance.BeginExpedition);
        }

        private IEnumerator FadeOut()
        {
            float startPromptAlpha = promptGroup.alpha;

            for (float time = 0f; time < fadeDuration; time += Time.deltaTime)
            {
                float t = time / fadeDuration;
                titleGroup.alpha = 1f - t;
                promptGroup.alpha = Mathf.Lerp(startPromptAlpha, 0f, t);
                yield return null;
            }

            titleGroup.alpha = 0f;
            promptGroup.alpha = 0f;
        }
    }
}
