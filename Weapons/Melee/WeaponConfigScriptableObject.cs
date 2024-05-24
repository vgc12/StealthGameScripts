using UnityEngine;

namespace Weapons.Melee
{
    public class WeaponConfigScriptableObject : ScriptableObject
    {
        public LayerMask HitMask;

        public float AttackDelay = 0.5f;

        public float Range = 1f;

        public int Damage = 1;
    }
}
