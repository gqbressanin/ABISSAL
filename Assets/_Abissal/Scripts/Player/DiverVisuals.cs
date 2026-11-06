using Abissal.World;
using UnityEngine;

namespace Abissal.Player
{
    /// <summary>
    /// Aparência do mergulhador: virar para os lados, inclinar, nadar e soltar bolhas de respiração.
    /// Fica separada do movimento porque só altera o objeto Visual, nunca o colisor.
    /// </summary>
    [RequireComponent(typeof(DiverController))]
    public class DiverVisuals : MonoBehaviour
    {
        private const float MovingThreshold = 0.5f;

        [SerializeField] private Transform visual;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private SpriteFrameAnimator animator;
        [SerializeField] private ParticleSystem breathBubbles;

        [Header("Nado")]
        [SerializeField, Min(0.1f)] private float idleFramesPerSecond = 3f;
        [SerializeField, Min(0.1f)] private float swimFramesPerSecond = 11f;
        [SerializeField, Range(0f, 90f)] private float maxTilt = 60f;
        [SerializeField, Min(0.1f)] private float tiltSpeed = 8f;

        [Header("Flutuação parada")]
        [SerializeField, Min(0f)] private float floatHeight = 0.07f;
        [SerializeField, Min(0f)] private float floatSpeed = 2f;

        [Header("Dano")]
        [SerializeField] private Color hitColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField, Min(0f)] private float hitFlashSeconds = 0.2f;
        [SerializeField, Min(0.1f)] private float blinksPerSecond = 12f;

        [Header("Respiração")]
        [SerializeField, Min(0.1f)] private float breathInterval = 2.6f;
        [SerializeField, Min(1)] private int breathBubbleCount = 3;

        private DiverController controller;
        private PlayerHealth health;
        private float hitTime = float.NegativeInfinity;
        private bool facingLeft;
        private float tilt;
        private float breathTimer;
        private float bubbleOffsetX;

        private void Awake()
        {
            controller = GetComponent<DiverController>();
            health = GetComponent<PlayerHealth>();
            health.Hit += OnHit;
            bubbleOffsetX = Mathf.Abs(breathBubbles.transform.localPosition.x);
        }

        private void OnDestroy()
        {
            health.Hit -= OnHit;
        }

        private void OnHit()
        {
            hitTime = Time.time;
        }

        private void Update()
        {
            UpdateDamageFeedback();

            if (!controller.HasControl)
            {
                return;
            }

            UpdateFacing();
            UpdateTilt();
            UpdateSwimming();
            UpdateBreathing();
        }

        /// <summary>Fica vermelho no impacto e pisca enquanto estiver invulnerável.</summary>
        private void UpdateDamageFeedback()
        {
            bool isFlashing = Time.time < hitTime + hitFlashSeconds;
            Color color = isFlashing ? hitColor : Color.white;

            if (health.IsInvulnerable && !isFlashing)
            {
                color.a = Mathf.Repeat(Time.time * blinksPerSecond, 1f) < 0.5f ? 0.35f : 1f;
            }

            spriteRenderer.color = color;
        }

        private void UpdateFacing()
        {
            float horizontal = controller.Velocity.x;

            if (Mathf.Abs(horizontal) > MovingThreshold)
            {
                facingLeft = horizontal < 0f;
                spriteRenderer.flipX = facingLeft;
            }
        }

        private void UpdateTilt()
        {
            Vector2 velocity = controller.Velocity;
            float targetTilt = 0f;

            if (velocity.magnitude > MovingThreshold)
            {
                float climbAngle = Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg;
                targetTilt = Mathf.Clamp(climbAngle, -maxTilt, maxTilt);
            }

            tilt = Mathf.Lerp(tilt, targetTilt, tiltSpeed * Time.deltaTime);
            visual.localRotation = Quaternion.Euler(0f, 0f, facingLeft ? -tilt : tilt);
        }

        private void UpdateSwimming()
        {
            float speed = Mathf.Clamp01(controller.SpeedRatio);
            animator.FramesPerSecond = Mathf.Lerp(idleFramesPerSecond, swimFramesPerSecond, speed);

            float bob = Mathf.Sin(Time.time * floatSpeed) * floatHeight * (1f - speed);
            visual.localPosition = Vector3.up * bob;
        }

        private void UpdateBreathing()
        {
            breathTimer += Time.deltaTime;

            if (breathTimer < breathInterval)
            {
                return;
            }

            breathTimer = 0f;
            Vector3 position = breathBubbles.transform.localPosition;
            position.x = facingLeft ? -bubbleOffsetX : bubbleOffsetX;
            breathBubbles.transform.localPosition = position;
            breathBubbles.Emit(breathBubbleCount);
        }
    }
}
