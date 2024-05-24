using System.Collections.Generic;
using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [DisallowMultipleComponent]
    public class PlayerGunSelector : MonoBehaviour
    {
        [SerializeField] private GunType Gun;
        [SerializeField] private Transform GunParent;
        [SerializeField] private List<GunScriptableObject> Guns;

        [Space]
        public GunScriptableObject CurrentGun;
        private void Start()
        {
            var gun = Guns.Find(gun => gun.GunType == Gun);
            if (gun == null)
            {
                Debug.LogError($"Gun not found for GunType: {gun}");
                return;
            }

            CurrentGun = gun;
       

            CurrentGun.Spawn(GunParent, this);
        
        }
    }
}
