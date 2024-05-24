using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Shoot Configuration", menuName = "Guns/Shoot Configuration", order = 1)]
    public class ShootConfigurationScriptableObject : ScriptableObject
    {
        public LayerMask hitMask;

        public Vector3 Spread = new(0.1f, 0.1f, 0.1f);

        public float FireRate = 0.5f;

    }
}
