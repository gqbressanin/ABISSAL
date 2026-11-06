using Abissal.Player;
using UnityEngine;

namespace Abissal.Hazards
{
    /// <summary>
    /// Qualquer objeto perigoso. Precisa de um Collider2D marcado como Trigger.
    /// Usa OnTriggerStay para ferir de novo se o mergulhador continuar encostado.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.attachedRigidbody != null && other.attachedRigidbody.TryGetComponent(out PlayerHealth health))
            {
                health.TakeHit(transform.position);
            }
        }
    }
}
