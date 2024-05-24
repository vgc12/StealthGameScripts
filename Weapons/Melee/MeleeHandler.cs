using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

//Fix This Script to not use Async for no reason
namespace Weapons.Melee
{
    public class MeleeHandler : MonoBehaviour
    {


        public MeleeWeaponScriptableObject currentMeleeWeapon;
        public RangedWeaponScriptableObject currentRangedWeapon;
        private int currentWeaponIndex;

        [SerializeField] private List<WeaponScriptableObject> availableWeapons;

        public Transform weaponParent;

        private bool attackPressed;

        public UnityEvent swingEvent;

        private RaycastHit hit;
        
        private Type weaponType;

        // Start is called before the first frame update
        private void Start()
        {
            currentWeaponIndex = 0;
            
            currentMeleeWeapon = availableWeapons.First(w => w.weaponType == WeaponType.Melee) as MeleeWeaponScriptableObject;
        
            
        
            currentRangedWeapon = availableWeapons.First(w => w.weaponType == WeaponType.Ranged) as RangedWeaponScriptableObject;
            if (currentRangedWeapon != null)
            {
                currentRangedWeapon.Spawn(weaponParent, this);
                currentRangedWeapon.Disable();
            }
            if(currentMeleeWeapon != null)
            {
                currentMeleeWeapon.Spawn(weaponParent, this);
            }

        }

        private void Update()
        {
            if (currentMeleeWeapon.Equipped)
            {
                Physics.Raycast(weaponParent.position, weaponParent.forward, out hit,
                    currentMeleeWeapon.weaponConfig.Range, currentMeleeWeapon.weaponConfig.HitMask);




                if (!attackPressed) return;
                attackPressed = false;
                currentMeleeWeapon.Attack(hit);
               
            }
        }


        public void ChangeWeapon(InputAction.CallbackContext ctx)
        {
            if(!enabled) return;
            var direction = ctx.ReadValue<float>();
            currentWeaponIndex += (int)direction;
            if (currentWeaponIndex < 0)
            {
                currentWeaponIndex = availableWeapons.Count - 1;
            }
            else if (currentWeaponIndex >= availableWeapons.Count)
            {
                currentWeaponIndex = 0;
            }
            
            
            SwitchToWeapon(currentWeaponIndex);
       

           

        }

        public void SwitchToWeapon(int index)
        {
            var weapon = availableWeapons[index];
           
        
        
            if (weapon.weaponType == WeaponType.Melee)
            {
             
                currentRangedWeapon.Disable();
                currentMeleeWeapon = (MeleeWeaponScriptableObject)weapon;
                currentMeleeWeapon.Enable();
                currentMeleeWeapon.ResetWeaponLocation();
            }
            else
            {
                
                currentMeleeWeapon.Disable();
                currentRangedWeapon = (RangedWeaponScriptableObject)weapon;
                currentRangedWeapon.Enable();
                currentRangedWeapon.ResetWeaponLocation();
            }
        }


        public void AttackPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                attackPressed = true;
            }
            else if (ctx.canceled)
            {
                attackPressed = false;
            }
        }


        
    }
}
