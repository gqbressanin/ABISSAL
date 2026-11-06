using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Abissal.Systems
{
    /// <summary>
    /// Escurece o oceano conforme a profundidade e acende a lanterna do mergulhador.
    /// Nas curvas, o eixo horizontal vai de 0 (superfície) a 1 (fundo).
    /// </summary>
    public class ZoneLighting : MonoBehaviour
    {
        [SerializeField] private DepthTracker depthTracker;
        [SerializeField] private Light2D globalLight;
        [SerializeField] private Light2D lantern;
        [SerializeField] private SpriteRenderer waterBackdrop;

        [SerializeField] private AnimationCurve globalIntensity = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.3f, 0.8f), new Keyframe(0.7f, 0.3f), new Keyframe(1f, 0.1f));

        [SerializeField] private AnimationCurve lanternIntensity = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.25f, 0f), new Keyframe(0.7f, 1f), new Keyframe(1f, 1.3f));

        [SerializeField] private Gradient waterTint = CreateDefaultTint();

        private void Update()
        {
            float depth = depthTracker.NormalizedDepth;

            globalLight.intensity = globalIntensity.Evaluate(depth);
            lantern.intensity = lanternIntensity.Evaluate(depth);
            waterBackdrop.color = waterTint.Evaluate(depth);
        }

        private static Gradient CreateDefaultTint()
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.55f, 0.7f, 0.95f), 0.3f),
                    new GradientColorKey(new Color(0.3f, 0.35f, 0.7f), 0.7f),
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.45f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
