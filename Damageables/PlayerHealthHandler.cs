using UnityEngine;

namespace Damageables
{
    public class PlayerHealthHandler : HealthHandler
    {
        public Transform[] targets;
        public bool IsDead { get; private set; }
        private new void Start()
        {
            base.Start();
            healthScriptableObject.Death += () => IsDead = true;
            
        }
    }
}