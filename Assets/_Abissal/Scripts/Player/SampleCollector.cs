using Abissal.Expedition;
using UnityEngine;

namespace Abissal.Player
{
    /// <summary>Procura amostras próximas e coleta com ESPAÇO.</summary>
    [RequireComponent(typeof(DiverController))]
    public class SampleCollector : MonoBehaviour
    {
        [SerializeField] private SampleTracker tracker;
        [SerializeField, Min(0.1f)] private float collectRange = 2.2f;

        private DiverController controller;

        /// <summary>Amostra ao alcance agora. O HUD usa isto para mostrar a instrução de coleta.</summary>
        public SampleItem NearbySample { get; private set; }

        private void Awake()
        {
            controller = GetComponent<DiverController>();
        }

        private void Update()
        {
            NearbySample = controller.HasControl ? tracker.FindNearest(transform.position, collectRange) : null;

            if (NearbySample != null && Input.GetKeyDown(KeyCode.Space))
            {
                tracker.Collect(NearbySample);
            }
        }
    }
}
