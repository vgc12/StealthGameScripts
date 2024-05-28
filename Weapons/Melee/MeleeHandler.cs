using System;
using System.Collections;
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
        private int _currentWeaponIndex;

        [SerializeField] private List<WeaponScriptableObject> availableWeapons;

        public Transform weaponParent;

        public Transform raycastOrigin;

        private bool _attackPressed;

        public UnityEvent swingEvent;


        
        private Type _weaponType;


        private void Start()
        {
            _currentWeaponIndex = 0;
            
            currentMeleeWeapon = availableWeapons.First(w => w.weaponType == WeaponType.Melee) as MeleeWeaponScriptableObject;
            
            currentRangedWeapon = availableWeapons.First(w => w.weaponType == WeaponType.Ranged) as RangedWeaponScriptableObject;

            raycastOrigin = transform.root;
            
            if (currentRangedWeapon != null)
            {
                currentRangedWeapon.Spawn(weaponParent, this);
                currentRangedWeapon.Equipped = false;
            }
            if(currentMeleeWeapon != null)
            {
                currentMeleeWeapon.Spawn(weaponParent, this);
            }

            WeaponScriptableObject.PickedUp += OnPickedUp;
        }
        private void OnPickedUp(WeaponScriptableObject weapon)
        {
            AddWeapon(weapon);
        }

        private void OnDrawGizmos()
        {
            if(raycastOrigin == null) return;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(raycastOrigin.position, raycastOrigin.forward * currentMeleeWeapon.weaponConfig.range);
            Gizmos.DrawSphere(_hit.point, 0.9f);
        }

        private RaycastHit _hit;
        private void Update()
        {
            if (currentMeleeWeapon.Equipped)
            {
                Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out _hit,
                    currentMeleeWeapon.weaponConfig.range, currentMeleeWeapon.weaponConfig.hitMask);

                if (!_attackPressed)
                {
                    return;
                }
                
                _attackPressed = false;
                currentMeleeWeapon.Attack(_hit);
               
            }
            else if (currentRangedWeapon.Equipped)     {
                Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out _hit,
                    currentRangedWeapon.weaponConfig.range, ~LayerMask.GetMask("Triggers"));

                
                
                if (!_attackPressed)
                {
                    return;
                }

                _attackPressed = false;
                currentRangedWeapon.Attack(_hit);
                StartCoroutine(SwitchFromRangedWeapon(.4f));
            }
        }

        private void AddWeapon(WeaponScriptableObject weapon)
        {
            if (!availableWeapons.Contains(weapon))
            {
                availableWeapons.Add(weapon);
            }

            SwitchToWeapon(availableWeapons.IndexOf(weapon));
            
        }

        public void ChangeWeapon(InputAction.CallbackContext ctx)
        {
            if(!enabled) return;
            var direction = ctx.ReadValue<float>();
            _currentWeaponIndex += (int)direction;
            if (_currentWeaponIndex < 0)
            {
                _currentWeaponIndex = availableWeapons.Count - 1;
            }
            else if (_currentWeaponIndex >= availableWeapons.Count)
            {
                _currentWeaponIndex = 0;
            }
            
            
            SwitchToWeapon(_currentWeaponIndex);
       

           

        }

       
        public void SwitchToWeapon(int index)
        {
            var weapon = availableWeapons[index];

            _currentWeaponIndex = index;
        
        
            if (weapon.weaponType == WeaponType.Melee)
            {

                if(!currentRangedWeapon.Thrown)             
                    currentRangedWeapon.Equipped = false;
               

                currentMeleeWeapon = weapon as MeleeWeaponScriptableObject;
                if (!currentMeleeWeapon) return;
                currentMeleeWeapon.Equipped = true;
                currentMeleeWeapon.ResetWeaponLocation();
            }
            else if (!currentRangedWeapon.Thrown)
            {

                currentMeleeWeapon.Equipped = false;
                currentRangedWeapon = weapon as RangedWeaponScriptableObject;
                if (!currentRangedWeapon) return;
                currentRangedWeapon.Equipped = true;
                currentRangedWeapon.ResetWeaponLocation();
            }
        }


        public void AttackPressed(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                _attackPressed = true;
            }
            else if (ctx.canceled)
            {
                _attackPressed = false;
            }
        }

        private IEnumerator SwitchFromRangedWeapon(float seconds)
        {
            
            yield return new WaitForSeconds(seconds);
            SwitchToWeapon(availableWeapons.IndexOf(currentMeleeWeapon));
        }

        
    }
}
