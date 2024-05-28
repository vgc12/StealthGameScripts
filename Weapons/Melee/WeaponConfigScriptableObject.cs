using UnityEngine;

namespace Weapons.Melee
{
    public class WeaponConfigScriptableObject : ScriptableObject
    {
        public LayerMask hitMask;

        public float range = 1f;

        public int damage = 1;
    }
}
