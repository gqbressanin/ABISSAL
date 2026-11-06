using System;
using System.Collections;
using UnityEngine;

namespace Abissal.World
{
    /// <summary>
    /// Sequência do início da expedição: o mergulhador salta do barco,
    /// entra na água e a câmera o acompanha até a posição inicial.
    /// </summary>
    public class DiveIntro : MonoBehaviour
    {
        private const float HeadUpAngle = 90f;
        private const float HeadDownAngle = -90f;

        [Header("Objetos")]
        [SerializeField] private Transform diver;
        [SerializeField] private SpriteFrameAnimator diverAnimator;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private ParticleSystem splash;
        [SerializeField] private ParticleSystem bubbleTrail;

        [Header("Salto")]
        [SerializeField] private Vector2 waterEntryPoint = new Vector2(10.2f, -0.4f);
        [SerializeField, Min(0f)] private float jumpHeight = 1.4f;
        [SerializeField, Min(0.1f)] private float jumpDuration = 0.8f;

        [Header("Descida")]
        [SerializeField] private Vector2 expeditionStartPoint = new Vector2(10.2f, -8.5f);
        [SerializeField, Min(0.1f)] private float descentDuration = 3.2f;

        public void Play(Action onComplete)
        {
            StartCoroutine(PlayRoutine(onComplete));
        }

        private IEnumerator PlayRoutine(Action onComplete)
        {
            yield return Jump();

            splash.transform.position = diver.position;
            splash.Play();
            bubbleTrail.Play();
            diverAnimator.FramesPerSecond = 9f;

            yield return Descend();

            bubbleTrail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            onComplete?.Invoke();
        }

        private IEnumerator Jump()
        {
            diver.SetParent(null, true);
            Vector3 start = diver.position;
            Vector3 end = new Vector3(waterEntryPoint.x, waterEntryPoint.y, start.z);

            for (float time = 0f; time < jumpDuration; time += Time.deltaTime)
            {
                float t = time / jumpDuration;
                Vector3 position = Vector3.Lerp(start, end, t);
                position.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

                diver.position = position;
                diver.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(HeadUpAngle, HeadDownAngle, t));
                yield return null;
            }

            diver.position = end;
            diver.rotation = Quaternion.Euler(0f, 0f, HeadDownAngle);
        }

        private IEnumerator Descend()
        {
            Vector3 diverStart = diver.position;
            Vector3 diverEnd = new Vector3(expeditionStartPoint.x, expeditionStartPoint.y, diverStart.z);
            Vector3 cameraStart = cameraTransform.position;
            Vector3 cameraEnd = new Vector3(expeditionStartPoint.x, expeditionStartPoint.y, cameraStart.z);

            for (float time = 0f; time < descentDuration; time += Time.deltaTime)
            {
                float t = time / descentDuration;
                float eased = Mathf.SmoothStep(0f, 1f, t);

                diver.position = Vector3.Lerp(diverStart, diverEnd, eased);
                cameraTransform.position = Vector3.Lerp(cameraStart, cameraEnd, eased);

                // Nos últimos 40% da descida o mergulhador gira para a posição horizontal.
                float turn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.6f, 1f, t));
                diver.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(HeadDownAngle, 0f, turn));
                yield return null;
            }

            diver.position = diverEnd;
            diver.rotation = Quaternion.identity;
            cameraTransform.position = cameraEnd;
        }
    }
}
