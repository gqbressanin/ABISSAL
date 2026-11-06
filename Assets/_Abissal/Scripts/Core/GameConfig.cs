using UnityEngine;

namespace Abissal.Core
{
    /// <summary>
    /// Valores de balanceamento do jogo, editáveis no Inspector sem mexer em código.
    /// O arquivo fica em Assets/_Abissal/Data/GameConfig.asset.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "ABISSAL/Configuração do Jogo")]
    public class GameConfig : ScriptableObject
    {
        [Header("Movimento do mergulhador")]
        [Tooltip("Velocidade máxima, em unidades por segundo.")]
        [Min(0.1f)] public float maxSpeed = 6f;
        [Tooltip("Quanto a velocidade aumenta por segundo enquanto há tecla pressionada.")]
        [Min(0.1f)] public float acceleration = 20f;
        [Tooltip("Quanto a velocidade diminui por segundo ao soltar as teclas. Menor = mais flutuação.")]
        [Min(0.1f)] public float deceleration = 12f;

        [Header("Oxigênio e vidas")]
        [Tooltip("Duração total do oxigênio, em segundos.")]
        [Min(1f)] public float oxygenSeconds = 240f;
        [Tooltip("Abaixo desta fração, a barra de oxigênio entra em alerta.")]
        [Range(0f, 1f)] public float lowOxygenWarning = 0.25f;
        [Min(1)] public int startingLives = 3;
        [Tooltip("Tempo sem receber dano depois de uma colisão, em segundos.")]
        [Min(0f)] public float invulnerabilitySeconds = 1.5f;
        [Tooltip("Velocidade com que o mergulhador é empurrado ao sofrer uma colisão.")]
        [Min(0f)] public float hitKnockbackSpeed = 7f;

        [Header("Profundidade")]
        [Tooltip("Quantos metros vale cada unidade do mundo.")]
        [Min(0.1f)] public float metersPerUnit = 5f;
        [Min(1f)] public float maxDepthMeters = 1000f;

        [Header("Limites do mundo")]
        public float worldLeft = -60f;
        public float worldRight = 60f;
        public float worldBottom = -204f;
        [Tooltip("Altura máxima do mergulhador: logo abaixo da linha da água.")]
        public float waterTop = -0.6f;
        [Tooltip("Limite superior da câmera, que deixa o céu aparecer perto da superfície.")]
        public float cameraTop = 2.44f;

        [Header("Câmera")]
        [Tooltip("Tempo aproximado para a câmera alcançar o mergulhador. Menor = mais rápida.")]
        [Min(0.01f)] public float cameraSmoothTime = 0.2f;
        [Tooltip("Quantas unidades a câmera se adianta na direção do movimento.")]
        [Min(0f)] public float cameraLookAhead = 1.5f;
    }
}
