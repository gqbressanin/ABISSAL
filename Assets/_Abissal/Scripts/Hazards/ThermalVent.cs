using UnityEngine;

namespace Abissal.Hazards
{
    /// <summary>
    /// Fonte termal em ciclo: descanso, aviso com poucas bolhas e erupção.
    /// A área de dano só fica ligada durante a erupção.
    /// </summary>
    public class ThermalVent : MonoBehaviour
    {
        [SerializeField] private Collider2D damageZone;
        [SerializeField] private ParticleSystem plume;

        [SerializeField, Min(0f)] private float restSeconds = 3f;
        [SerializeField, Min(0f)] private float warningSeconds = 1f;
        [SerializeField, Min(0.1f)] private float eruptionSeconds = 2f;

        [SerializeField, Min(0f)] private float warningParticlesPerSecond = 8f;
        [SerializeField, Min(0f)] private float eruptionParticlesPerSecond = 70f;

        private float cycleOffset;

        private void Awake()
        {
            cycleOffset = Random.value * 10f;
            damageZone.enabled = false;
        }

        private void Update()
        {
            float cycleLength = restSeconds + warningSeconds + eruptionSeconds;
            float position = Mathf.Repeat(Time.time + cycleOffset, cycleLength);

            bool isErupting = position >= restSeconds + warningSeconds;
            bool isWarning = !isErupting && position >= restSeconds;

            damageZone.enabled = isErupting;

            ParticleSystem.EmissionModule emission = plume.emission;
            emission.rateOverTime = isErupting ? eruptionParticlesPerSecond : isWarning ? warningParticlesPerSecond : 0f;
        }
    }
}
