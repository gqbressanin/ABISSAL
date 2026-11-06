using Abissal.Core;
using Abissal.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Abissal.UI
{
    /// <summary>Mostrador provisório de profundidade. Será substituído pelo HUD final.</summary>
    [RequireComponent(typeof(Text))]
    public class DepthReadout : MonoBehaviour
    {
        [SerializeField] private DepthTracker depthTracker;

        private Text label;

        private void Awake()
        {
            label = GetComponent<Text>();
            label.enabled = false;
        }

        private void Update()
        {
            label.enabled = GameManager.Instance.State == GameState.Playing;
            label.text = $"{depthTracker.CurrentDepth:F0} m";
        }
    }
}
