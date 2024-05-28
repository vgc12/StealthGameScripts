using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "Attack Configuration", menuName = "Enemy Configuration / Attack Configuration", order = 0)]
    public class AttackConfigScriptableObject : ScriptableObject
    {
        public int damage;
        public float attackRange;
        public float attackCooldown;
    }
}
