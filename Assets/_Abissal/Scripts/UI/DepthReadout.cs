using Abissal.Systems;
using UnityEngine;

namespace Abissal.UI
{
    /// <summary>Mostra a profundidade atual no HUD.</summary>
    [RequireComponent(typeof(PixelText))]
    public class DepthReadout : MonoBehaviour
    {
        [SerializeField] private DepthTracker depthTracker;

        private PixelText label;

        private void Awake()
        {
            label = GetComponent<PixelText>();
        }

        private void Update()
        {
            label.SetText($"{depthTracker.CurrentDepth:F0} m");
        }
    }
}
