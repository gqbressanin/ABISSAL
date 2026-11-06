using UnityEngine;

namespace Abissal.Expedition
{
    /// <summary>
    /// Ativa perigos extras quando a amostra principal é coletada,
    /// para que a volta seja mais tensa do que a descida.
    /// </summary>
    public class ReturnTension : MonoBehaviour
    {
        [SerializeField] private SampleTracker sampleTracker;
        [SerializeField] private GameObject returnHazards;

        private void Awake()
        {
            returnHazards.SetActive(false);
        }

        private void OnEnable()
        {
            sampleTracker.SampleCollected += OnSampleCollected;
        }

        private void OnDisable()
        {
            sampleTracker.SampleCollected -= OnSampleCollected;
        }

        private void OnSampleCollected(SampleItem sample)
        {
            if (sample.IsMainSample)
            {
                returnHazards.SetActive(true);
            }
        }
    }
}
