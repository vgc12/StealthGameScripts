using UnityEngine;

namespace Weapons.Guns.Ammo
{
    [CreateAssetMenu(fileName = "Ammo", menuName = "Guns/Ammo", order = 0)]
    public class AmmoScriptableObject : ScriptableObject
    {
        public float Damage = 10;

        public int ClipSize = 30;

        public int CurrentAmmo = 30;

        public int MaxAmmo = 90;

        public int ReserveAmmo = 60;

        public bool IsEmpty;

        public BulletType BulletType;

    

        public void Spawn()
        {
            CurrentAmmo = ClipSize;

        }


        public void SubtractAmmo()
        {
            CurrentAmmo--;
        }

        public void Reload(BulletType bulletType = BulletType.Normal)
        {
            bulletType = BulletType;
        }

    
    }
}
