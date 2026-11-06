using System;
using Abissal.Core;
using UnityEngine;

namespace Abissal.Player
{
    /// <summary>Controla as vidas e o período de invulnerabilidade depois de uma colisão.</summary>
    [RequireComponent(typeof(DiverController))]
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private DiverController controller;
        private float invulnerableUntil;

        public int Lives { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;

        /// <summary>Avisa que o mergulhador perdeu uma vida.</summary>
        public event Action Hit;

        private void Awake()
        {
            controller = GetComponent<DiverController>();
            Lives = config.startingLives;
        }

        /// <summary>Tira uma vida. A colisão é ignorada se ele estiver invulnerável.</summary>
        /// <param name="hazardPosition">Posição do perigo, usada para empurrar o mergulhador para longe.</param>
        public void TakeHit(Vector2 hazardPosition)
        {
            if (IsInvulnerable || GameManager.Instance.State != GameState.Playing)
            {
                return;
            }

            Lives--;
            invulnerableUntil = Time.time + config.invulnerabilitySeconds;
            controller.Knockback(((Vector2)transform.position - hazardPosition).normalized);
            Hit?.Invoke();

            if (Lives <= 0)
            {
                GameManager.Instance.LoseExpedition(GameOverReason.OutOfLives);
            }
        }
    }
}
