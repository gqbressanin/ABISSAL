using Abissal.Core;
using Abissal.Player;
using UnityEngine;

namespace Abissal.World
{
    /// <summary>Segue o mergulhador com suavidade, olhando um pouco à frente, sem sair dos limites do mundo.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameConfig config;
        [SerializeField] private DiverController target;

        private Camera followCamera;
        private Vector3 smoothVelocity;

        private void Awake()
        {
            followCamera = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (!target.HasControl)
            {
                return;
            }

            Vector2 lookAhead = target.Velocity / config.maxSpeed * config.cameraLookAhead;
            Vector3 desired = ClampToWorld((Vector2)target.transform.position + lookAhead);
            desired.z = transform.position.z;

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref smoothVelocity, config.cameraSmoothTime);
        }

        private Vector3 ClampToWorld(Vector2 position)
        {
            float halfHeight = followCamera.orthographicSize;
            float halfWidth = halfHeight * followCamera.aspect;

            return new Vector3(
                ClampCentered(position.x, config.worldLeft + halfWidth, config.worldRight - halfWidth),
                ClampCentered(position.y, config.worldBottom + halfHeight, config.cameraTop - halfHeight),
                0f);
        }

        /// <summary>Se o mundo for menor que a tela, a câmera fica centralizada em vez de oscilar.</summary>
        private static float ClampCentered(float value, float min, float max)
        {
            return min > max ? (min + max) * 0.5f : Mathf.Clamp(value, min, max);
        }
    }
}
