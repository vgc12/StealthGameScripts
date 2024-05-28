using System;
using UnityEngine;
using Weapons.Melee;

namespace PlayerScripts.Input.Combat
{
    [RequireComponent(typeof(MeleeHandler))]
    public class PlayerCombat : MonoBehaviour
    {
        
        
        private PlayerInputActions _playerInputActions;
        
        public MeleeHandler meleeHandler;
        
        private PlayerBlocking _playerBlocking;

        private Collider[] _weaponDetectionColliders = new Collider[1];
        


        private void Awake()
        {
            meleeHandler = GetComponent<MeleeHandler>();
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Fire.Enable();
            
            _playerInputActions.Player.Fire.performed += meleeHandler.AttackPressed;
            Player.Instance.PlayerStateChangedEvent += OnStateChanged;
            _playerInputActions.Player.SwitchWeapon.performed += meleeHandler.ChangeWeapon;
            
            _playerBlocking = GetComponent<PlayerBlocking>();
            _playerBlocking.enabled = false;
            _playerInputActions.Player.SwitchWeapon.Enable();

          
        }

      

        private void OnStateChanged(PlayerStateInfo info)
        {
            if (info.State == PlayerState.InCombat)
            {
                
                meleeHandler.SwitchToWeapon(0);
                _playerBlocking.enabled = true;
            }
            else
            {
                
                //meleeHandler.ResetWeaponLocation();
                meleeHandler.enabled = true;
                _playerBlocking.enabled = false;
            
            }
        }

        private void OnDestroy()
        {
            _playerInputActions.Player.Fire.performed -= meleeHandler.AttackPressed;
            _playerInputActions.Player.SwitchWeapon.performed -= meleeHandler.ChangeWeapon;
            
            Player.Instance.PlayerStateChangedEvent -= OnStateChanged;

        }
    }
}
