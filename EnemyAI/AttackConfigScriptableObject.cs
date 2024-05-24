using UnityEngine;

namespace EnemyAI
{
    [CreateAssetMenu(fileName = "Attack Configuration", menuName = "Enemy Configuration / Attack Configuration", order = 0)]
    public class AttackConfigScriptableObject : ScriptableObject
    {
        public int Damage;
        public float AttackRange;
        public float AttackCooldown;
    }
}
