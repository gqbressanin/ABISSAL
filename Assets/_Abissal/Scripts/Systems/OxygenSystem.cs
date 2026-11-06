using Abissal.Core;
using UnityEngine;

namespace Abissal.Systems
{
    /// <summary>Consome o oxigênio durante a expedição. Ele nunca é recuperado.</summary>
    public class OxygenSystem : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        public float RemainingSeconds { get; private set; }

        /// <summary>Oxigênio restante, de 0 (vazio) a 1 (cheio).</summary>
        public float Normalized => RemainingSeconds / config.oxygenSeconds;

        public bool IsLow => Normalized <= config.lowOxygenWarning;

        private void Awake()
        {
            RemainingSeconds = config.oxygenSeconds;
        }

        private void Update()
        {
            if (GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            RemainingSeconds = Mathf.Max(0f, RemainingSeconds - Time.deltaTime);

            if (RemainingSeconds <= 0f)
            {
                GameManager.Instance.LoseExpedition(GameOverReason.OutOfOxygen);
            }
        }
    }
}
