using Abissal.Core;
using UnityEngine;

namespace Abissal.Player
{
    /// <summary>
    /// Movimentação livre do mergulhador nas quatro direções.
    /// O Rigidbody2D fica desligado até a expedição começar, pois antes disso quem move é a DiveIntro.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DiverController : MonoBehaviour
    {
        [SerializeField] private GameConfig config;

        private Rigidbody2D body;
        private Vector2 moveInput;

        public bool HasControl { get; private set; }
        public Vector2 Velocity => body.velocity;
        public float SpeedRatio => body.velocity.magnitude / config.maxSpeed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.simulated = false;
        }

        private void Start()
        {
            GameManager.Instance.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StateChanged -= OnStateChanged;
            }
        }

        private void Update()
        {
            moveInput = HasControl ? ReadInput() : Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (HasControl)
            {
                ApplyMovement(moveInput, Time.fixedDeltaTime);
            }
        }

        /// <summary>Empurra o mergulhador. A aceleração normal devolve o controle em seguida.</summary>
        public void Knockback(Vector2 direction)
        {
            body.velocity = direction * config.hitKnockbackSpeed;
        }

        private void OnStateChanged(GameState state)
        {
            HasControl = state == GameState.Playing;

            // Na pausa o tempo já está parado, e a velocidade precisa ser mantida para a volta.
            if (state == GameState.Paused)
            {
                return;
            }

            body.simulated = HasControl;

            if (!HasControl)
            {
                body.velocity = Vector2.zero;
            }
        }

        private static Vector2 ReadInput()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            // Sem este limite, andar na diagonal seria cerca de 41% mais rápido.
            return Vector2.ClampMagnitude(input, 1f);
        }

        private void ApplyMovement(Vector2 input, float deltaTime)
        {
            float rate = input == Vector2.zero ? config.deceleration : config.acceleration;
            Vector2 velocity = Vector2.MoveTowards(body.velocity, input * config.maxSpeed, rate * deltaTime);
            body.velocity = KeepInsideWorld(velocity);
        }

        /// <summary>Corta a velocidade que levaria o mergulhador para fora dos limites do mundo.</summary>
        private Vector2 KeepInsideWorld(Vector2 velocity)
        {
            Vector2 position = body.position;

            if ((position.x <= config.worldLeft && velocity.x < 0f) || (position.x >= config.worldRight && velocity.x > 0f))
            {
                velocity.x = 0f;
            }

            if ((position.y <= config.worldBottom && velocity.y < 0f) || (position.y >= config.waterTop && velocity.y > 0f))
            {
                velocity.y = 0f;
            }

            return velocity;
        }
    }
}
