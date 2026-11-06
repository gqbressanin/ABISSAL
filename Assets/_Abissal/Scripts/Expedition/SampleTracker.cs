using System;
using System.Collections.Generic;
using UnityEngine;

namespace Abissal.Expedition
{
    /// <summary>
    /// Registra as amostras coletadas. O total é contado na própria cena,
    /// então adicionar uma amostra nova não exige mudar nenhum código.
    /// </summary>
    public class SampleTracker : MonoBehaviour
    {
        private readonly List<SampleItem> samples = new List<SampleItem>();
        private readonly HashSet<string> collectedIds = new HashSet<string>();

        public int SecondaryTotal { get; private set; }
        public int SecondaryCollected { get; private set; }
        public bool HasMainSample { get; private set; }

        public int CompletionPercent => ExpeditionRules.CompletionPercent(HasMainSample, SecondaryCollected, SecondaryTotal);

        /// <summary>Avisa que uma amostra acabou de ser coletada.</summary>
        public event Action<SampleItem> SampleCollected;

        private void Awake()
        {
            var knownIds = new HashSet<string>();

            foreach (SampleItem sample in FindObjectsOfType<SampleItem>())
            {
                if (!knownIds.Add(sample.SampleId))
                {
                    Debug.LogError($"Amostra com identificador repetido: {sample.SampleId}", sample);
                }

                samples.Add(sample);

                if (!sample.IsMainSample)
                {
                    SecondaryTotal++;
                }
            }
        }

        public bool IsCollected(SampleItem sample)
        {
            return collectedIds.Contains(sample.SampleId);
        }

        /// <summary>Retorna a amostra não coletada mais próxima dentro do alcance, ou null.</summary>
        public SampleItem FindNearest(Vector2 position, float range)
        {
            SampleItem nearest = null;
            float nearestDistance = range;

            foreach (SampleItem sample in samples)
            {
                float distance = Vector2.Distance(position, sample.transform.position);

                if (distance <= nearestDistance && !IsCollected(sample))
                {
                    nearest = sample;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /// <summary>Registra a coleta. Retorna falso se a amostra já tiver sido coletada.</summary>
        public bool Collect(SampleItem sample)
        {
            if (!collectedIds.Add(sample.SampleId))
            {
                return false;
            }

            if (sample.IsMainSample)
            {
                HasMainSample = true;
            }
            else
            {
                SecondaryCollected++;
            }

            sample.PlayCollected();
            SampleCollected?.Invoke(sample);
            return true;
        }
    }
}
