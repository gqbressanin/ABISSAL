using UnityEngine;

namespace Abissal.Expedition
{
    /// <summary>Uma amostra científica no cenário. O registro da coleta fica no SampleTracker.</summary>
    public class SampleItem : MonoBehaviour
    {
        [Tooltip("Identificador único, usado para impedir que a amostra seja coletada duas vezes.")]
        [SerializeField] private string sampleId;
        [SerializeField] private bool isMainSample;
        [SerializeField] private SpriteRenderer[] visuals;
        [SerializeField] private Behaviour[] glowEffects;
        [SerializeField] private ParticleSystem collectBurst;
        [Tooltip("Opcional: partículas contínuas ao redor da amostra.")]
        [SerializeField] private ParticleSystem ambientParticles;

        public string SampleId => sampleId;
        public bool IsMainSample => isMainSample;

        /// <summary>Esconde a amostra e toca o efeito de coleta.</summary>
        public void PlayCollected()
        {
            foreach (SpriteRenderer visual in visuals)
            {
                visual.enabled = false;
            }

            foreach (Behaviour glow in glowEffects)
            {
                glow.enabled = false;
            }

            if (ambientParticles != null)
            {
                ambientParticles.Stop();
            }

            collectBurst.Play();
        }
    }
}
