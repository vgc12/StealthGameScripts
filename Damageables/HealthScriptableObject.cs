using UnityEngine;

namespace Damageables
{
    [CreateAssetMenu(fileName = "Health Configuration ScriptableObject", menuName = "Damageables/Health Configuration", order = 0)]
    public class HealthScriptableObject : ScriptableObject
    {
        public DamageableType damageableType;

        private float health;

        public float maxHealth;
        
        public HealthChangedEvent HealthChanged;
        public delegate void HealthChangedEvent(float currentHealth);

        public delegate void DeathEvent();
        public DeathEvent Death;
        

        public void Spawn()
        {
            health = maxHealth;
            
        }

        public void TakeDamage(float damage)
        {
            health -= damage;

            if (health <= 0)
            {
                health = 0;
                
                Death?.Invoke();
            }

            HealthChanged?.Invoke(health);
        }

    

  
    
    }
}
