using System;
using Abissal.Core;
using Abissal.Player;
using UnityEngine;

namespace Abissal.Expedition
{
    /// <summary>
    /// Faixa de água junto à superfície. É o único lugar onde a vitória pode acontecer.
    /// Precisa de um Collider2D marcado como Trigger.
    /// </summary>
    public class SurfaceZone : MonoBehaviour
    {
        [SerializeField] private SampleTracker sampleTracker;

        /// <summary>Avisa que o mergulhador chegou à superfície sem a amostra principal.</summary>
        public event Action ReachedWithoutMainSample;

        private void OnTriggerEnter2D(Collider2D other)
        {
            bool isDiver = other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out DiverController _);

            if (isDiver && !GameManager.Instance.TryWinExpedition(sampleTracker.HasMainSample))
            {
                ReachedWithoutMainSample?.Invoke();
            }
        }
    }
}
