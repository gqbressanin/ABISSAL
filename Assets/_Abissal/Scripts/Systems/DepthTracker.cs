using Abissal.Core;
using UnityEngine;

namespace Abissal.Systems
{
    /// <summary>Converte a altura do mergulhador em metros e guarda a profundidade máxima atingida.</summary>
    public class DepthTracker : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private Transform diver;

        public float CurrentDepth { get; private set; }
        public float MaxDepth { get; private set; }

        /// <summary>Profundidade de 0 (superfície) a 1 (fundo do oceano).</summary>
        public float NormalizedDepth => Mathf.Clamp01(CurrentDepth / config.maxDepthMeters);

        private void Update()
        {
            CurrentDepth = Mathf.Clamp(-diver.position.y * config.metersPerUnit, 0f, config.maxDepthMeters);
            MaxDepth = Mathf.Max(MaxDepth, CurrentDepth);
        }
    }
}
