using Abissal.Core;
using Abissal.Player;
using Abissal.World;
using UnityEngine;

namespace Abissal.Hazards
{
    public enum PredatorState
    {
        Patrol,
        Alert,
        Chase,
        Returning
    }

    /// <summary>
    /// Predador que patrulha, percebe o mergulhador e o persegue por alguns segundos.
    /// Desiste quando o tempo acaba ou depois de acertá-lo, e então volta para a patrulha.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PredatorChase : MonoBehaviour
    {
        private const float AlertFramesPerSecond = 12f;
        private const float ChaseFramesPerSecond = 10f;
        private const float CalmFramesPerSecond = 4f;
        private const float HitDistance = 3.5f;
        private const float HomeArrivalDistance = 0.5f;
        private const float SnapHomeDistance = 24f;
        private const float MaxTiltDegrees = 30f;

        [Header("Patrulha")]
        [SerializeField] private float minX = -18f;
        [SerializeField] private float maxX = 18f;
        [SerializeField, Min(0f)] private float patrolSpeed = 4f;
        [SerializeField, Min(0f)] private float bobHeight = 0.3f;

        [Header("Perseguição")]
        [SerializeField, Min(0f)] private float detectionRange = 9f;
        [SerializeField, Min(0f)] private float alertSeconds = 0.5f;
        [SerializeField, Min(0.1f)] private float chaseSeconds = 5f;
        [Tooltip("O mergulhador nada a 6. Acima disso, fugir em linha reta não basta.")]
        [SerializeField, Min(0f)] private float chaseSpeed = 6.6f;
        [Tooltip("Menor = demora mais para virar, o que permite desviar.")]
        [SerializeField, Min(0.1f)] private float turnAcceleration = 10f;
        [Tooltip("Tempo sem perceber o mergulhador depois de desistir.")]
        [SerializeField, Min(0f)] private float cooldownSeconds = 6f;

        [Header("Colisão com o cenário")]
        [SerializeField] private LayerMask terrainMask;
        [SerializeField, Min(0.1f)] private float bodyRadius = 0.6f;
        [SerializeField] private Color alertColor = new Color(1f, 0.5f, 0.45f);

        private SpriteRenderer spriteRenderer;
        private SpriteFrameAnimator animator;
        private PlayerHealth health;
        private Transform diver;
        private float homeY;
        private int patrolDirection = 1;
        private Vector2 velocity;
        private float stateEndTime;
        private float nextDetectionTime;

        public PredatorState State { get; private set; } = PredatorState.Patrol;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<SpriteFrameAnimator>();
            homeY = transform.position.y;
            patrolDirection = Random.value < 0.5f ? -1 : 1;
        }

        private void Start()
        {
            health = FindObjectOfType<PlayerHealth>();
            diver = health.transform;
            health.Hit += OnDiverHit;
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Hit -= OnDiverHit;
            }
        }

        private void Update()
        {
            GameState gameState = GameManager.Instance.State;
            bool isExpeditionRunning = gameState == GameState.Playing || gameState == GameState.Paused;

            if (State != PredatorState.Patrol && !isExpeditionRunning)
            {
                GiveUp();
            }

            switch (State)
            {
                case PredatorState.Patrol:
                    UpdatePatrol();
                    break;
                case PredatorState.Alert:
                    UpdateAlert();
                    break;
                case PredatorState.Chase:
                    UpdateChase();
                    break;
                case PredatorState.Returning:
                    UpdateReturning();
                    break;
            }

            UpdateAppearance();
        }

        private void UpdatePatrol()
        {
            Vector3 position = transform.position;
            position.x += patrolDirection * patrolSpeed * Time.deltaTime;
            position.y = homeY + Mathf.Sin(Time.time * 1.5f) * bobHeight;
            transform.position = position;

            if (position.x > maxX)
            {
                patrolDirection = -1;
            }
            else if (position.x < minX)
            {
                patrolDirection = 1;
            }

            velocity = new Vector2(patrolDirection * patrolSpeed, 0f);

            if (CanSeeDiver())
            {
                State = PredatorState.Alert;
                stateEndTime = Time.time + alertSeconds;
            }
        }

        private void UpdateAlert()
        {
            velocity = (diver.position - transform.position).normalized * 0.01f;

            if (Time.time >= stateEndTime)
            {
                State = PredatorState.Chase;
                stateEndTime = Time.time + chaseSeconds;
            }
        }

        private void UpdateChase()
        {
            Vector2 toDiver = (diver.position - transform.position).normalized;
            velocity = Vector2.MoveTowards(velocity, toDiver * chaseSpeed, turnAcceleration * Time.deltaTime);
            Move(velocity * Time.deltaTime);

            if (Time.time >= stateEndTime)
            {
                GiveUp();
            }
        }

        private void UpdateReturning()
        {
            Vector2 home = new Vector2(Mathf.Clamp(transform.position.x, minX, maxX), homeY);
            Vector2 toHome = home - (Vector2)transform.position;

            // Longe do jogador ninguém vê a volta, então um predador preso pode reaparecer na patrulha.
            bool isFarFromDiver = Vector2.Distance(transform.position, diver.position) > SnapHomeDistance;

            if (toHome.magnitude < HomeArrivalDistance || isFarFromDiver)
            {
                transform.position = home;
                State = PredatorState.Patrol;
                return;
            }

            velocity = toHome.normalized * patrolSpeed;
            Move(velocity * Time.deltaTime);

            if (CanSeeDiver())
            {
                State = PredatorState.Alert;
                stateEndTime = Time.time + alertSeconds;
            }
        }

        private void GiveUp()
        {
            State = PredatorState.Returning;
            nextDetectionTime = Time.time + cooldownSeconds;
        }

        private void OnDiverHit()
        {
            bool isClose = Vector2.Distance(transform.position, diver.position) < HitDistance;

            if (State == PredatorState.Chase && isClose)
            {
                GiveUp();
            }
        }

        private bool CanSeeDiver()
        {
            if (GameManager.Instance.State != GameState.Playing || Time.time < nextDetectionTime)
            {
                return false;
            }

            Vector2 position = transform.position;
            Vector2 target = diver.position;

            return Vector2.Distance(position, target) <= detectionRange
                && !Physics2D.Linecast(position, target, terrainMask);
        }

        /// <summary>Move um eixo de cada vez para deslizar nas paredes em vez de atravessá-las.</summary>
        private void Move(Vector2 delta)
        {
            Vector2 position = transform.position;

            if (IsFree(position, new Vector2(delta.x, 0f)))
            {
                position.x += delta.x;
            }

            if (IsFree(position, new Vector2(0f, delta.y)))
            {
                position.y += delta.y;
            }

            transform.position = position;
        }

        private bool IsFree(Vector2 position, Vector2 delta)
        {
            float distance = delta.magnitude;
            return distance <= 0f || !Physics2D.CircleCast(position, bodyRadius, delta / distance, distance, terrainMask);
        }

        private void UpdateAppearance()
        {
            bool isAggressive = State == PredatorState.Alert || State == PredatorState.Chase;

            spriteRenderer.color = isAggressive ? alertColor : Color.white;
            animator.FramesPerSecond = State == PredatorState.Alert ? AlertFramesPerSecond
                : State == PredatorState.Chase ? ChaseFramesPerSecond
                : CalmFramesPerSecond;

            if (Mathf.Abs(velocity.x) > 0.001f)
            {
                spriteRenderer.flipX = velocity.x < 0f;
            }

            float tilt = State == PredatorState.Chase
                ? Mathf.Clamp(Mathf.Atan2(velocity.y, Mathf.Abs(velocity.x)) * Mathf.Rad2Deg, -MaxTiltDegrees, MaxTiltDegrees)
                : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, spriteRenderer.flipX ? -tilt : tilt);
        }
    }
}
