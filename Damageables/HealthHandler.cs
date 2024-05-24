
using UnityEngine;

namespace Damageables
{
    public  class HealthHandler : MonoBehaviour
    {
   
        public HealthScriptableObject healthScriptableObject;

        // Start is called before the first frame update
        protected void Start()
        {
            
            healthScriptableObject.Spawn();
            healthScriptableObject.HealthChanged += OnHealthChanged;
        }

        protected void OnHealthChanged(float currentHealth)
        {
            
        }

        

        private void OnDisable()
        {
            healthScriptableObject.HealthChanged -= OnHealthChanged;
        }
    }
}
