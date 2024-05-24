using Damageables;
using UnityEngine;

namespace EnemyAI
{
    public class AttackSensor : MonoBehaviour
    {
        public SphereCollider Collider;
    
        public delegate void PlayerEntered(Transform player);

        public delegate void PlayerExited(Vector3 lastKnownPosition);

        public event PlayerEntered PlayerEnterEvent;

        public event PlayerExited PlayerExitEvent;
    
        public void OnTriggerEnter(Collider other)
        {
            if (!other.transform.root.TryGetComponent(out HealthHandler handler)) return;
        
            if(handler.healthScriptableObject.damageableType == DamageableType.Player)
            {
                PlayerEnterEvent?.Invoke(other.transform.root);
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (!other.transform.root.TryGetComponent(out HealthHandler handler)) return;
        
            if(handler.healthScriptableObject.damageableType == DamageableType.Player)
            {
                PlayerExitEvent?.Invoke(other.transform.root.position);
            }
        }
    }
}
