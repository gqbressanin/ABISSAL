using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abissal.Core
{
    /// <summary>
    /// Guarda o estado atual do jogo e avisa os outros sistemas quando ele muda.
    /// Os sistemas escutam o evento StateChanged em vez de se chamarem diretamente.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        /// <summary>Quando verdadeiro, a cena recarregada pula o título e vai direto ao mergulho.</summary>
        public static bool SkipTitleOnLoad { get; set; }

        public GameState State { get; private set; } = GameState.Title;
        public GameOverReason DefeatReason { get; private set; } = GameOverReason.None;

        public event Action<GameState> StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Chamado quando a transição da tela inicial termina.</summary>
        public void BeginExpedition()
        {
            if (State == GameState.Title)
            {
                SetState(GameState.Playing);
            }
        }

        public void LoseExpedition(GameOverReason reason)
        {
            if (State == GameState.Playing)
            {
                DefeatReason = reason;
                SetState(GameState.GameOver);
            }
        }

        /// <summary>Recarrega a cena, o que devolve oxigênio, vidas e amostras ao estado inicial.</summary>
        public void RestartExpedition()
        {
            SkipTitleOnLoad = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void SetState(GameState newState)
        {
            State = newState;
            StateChanged?.Invoke(newState);
        }
    }
}
