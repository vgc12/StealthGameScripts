using UnityEngine;

namespace Damageables
{
    [CreateAssetMenu(fileName = "Health Configuration ScriptableObject", menuName = "Damageables/Health Configuration", order = 0)]
    public class HealthScriptableObject : ScriptableObject
    {
        public DamageableType damageableType;

        private float _health;

        public float maxHealth;
        
        public HealthChangedEvent HealthChanged;
        public delegate void HealthChangedEvent(float currentHealth);

        public delegate void DeathEvent();
        public DeathEvent Death;
        

        public void Spawn()
        {
            _health = maxHealth;
            
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;

            if (_health <= 0)
            {
                _health = 0;
                
                Death?.Invoke();
            }

            HealthChanged?.Invoke(_health);
        }

    

  
    
    }
}
