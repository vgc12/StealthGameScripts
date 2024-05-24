using UnityEngine;
using Weapons.Melee;

namespace PlayerScripts.Input.Combat
{
    [RequireComponent(typeof(MeleeHandler))]
    public class PlayerCombat : MonoBehaviour
    {
        
        
        private PlayerInputActions playerInputActions;


        public MeleeHandler meleeHandler;
        
        private PlayerBlocking playerBlocking;

        


        private void Awake()
        {
            meleeHandler = GetComponent<MeleeHandler>();
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Fire.Enable();
            playerInputActions.Player.Fire.performed += meleeHandler.AttackPressed;
            Player.PlayerStateChangedEvent += OnStateChanged;
            playerBlocking = GetComponent<PlayerBlocking>();
            playerBlocking.enabled = false;
            playerInputActions.Player.SwitchWeapon.Enable();
            playerInputActions.Player.SwitchWeapon.performed += meleeHandler.ChangeWeapon;
        }


        private void OnStateChanged(PlayerStateInfo info)
        {
            if (info.State == PlayerState.Detected)
            {
                
                meleeHandler.SwitchToWeapon(0);
                playerBlocking.enabled = true;
            }
            else
            {
                
                //meleeHandler.ResetWeaponLocation();
                meleeHandler.enabled = true;
                playerBlocking.enabled = false;
            
            }
        }

        private void OnDestroy()
        {
            playerInputActions.Player.Fire.performed -= meleeHandler.AttackPressed;
            
        }
    }
}
