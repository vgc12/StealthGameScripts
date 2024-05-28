using Damageables;
using JetBrains.Annotations;
using PlayerScripts.Input.Combat;
using PlayerScripts.Input.Movement;
using UnityEngine;

namespace PlayerScripts
{
    public struct PlayerStateInfo
    {
        public PlayerStateInfo(PlayerState ps, Transform rotTarget, Transform cameraPosition)
        {
            State = ps;
            CameraRotationTarget = rotTarget;
            CameraPosition = cameraPosition;
        }

        public PlayerStateInfo(PlayerState ps, Transform rotTarget)
        {
           
            State = ps;
            CameraRotationTarget = rotTarget;
            CameraPosition = null;
     
        }
        public PlayerStateInfo(PlayerState ps)
        {
            State = ps;
            CameraRotationTarget = null;
            CameraPosition = null;
        }
        
        public readonly PlayerState State;
        [CanBeNull] public readonly Transform CameraRotationTarget;
        [CanBeNull] public readonly Transform CameraPosition;

    }
    public enum PlayerState
    {
        Undetected,
        InCombat,
        Hiding,
        Dead
    }
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
    
        public PlayerMovement PlayerMovement { get; private set; }
    
        public PlayerLooking PlayerLooking { get; private set; }

        private PlayerCombat PlayerCombat { get; set; }

        public PlayerHealthHandler HealthHandler { get; private set; }

        private CameraEffects CameraEffects { get; set; }

        private PlayerBlocking PlayerBlocking { get; set; }
        
        public PlayerState playerState;
        
        public delegate void PlayerStateChanged(PlayerStateInfo newStateInfo);
        
        public PlayerStateChanged PlayerStateChangedEvent;
        

      
        private void Awake()
        {
            if(Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                Instance.HealthHandler = GetComponent<PlayerHealthHandler>();
                Instance.PlayerMovement = GetComponent<PlayerMovement>();
                var cameraObject = GameObject.FindWithTag("MainCamera");
                Instance.PlayerCombat = cameraObject.GetComponentInChildren<PlayerCombat>();
                Instance.PlayerBlocking = cameraObject.GetComponentInChildren<PlayerBlocking>();
                Instance.PlayerLooking = cameraObject.GetComponentInParent<PlayerLooking>();
                Instance.CameraEffects = GetComponent<CameraEffects>();
                Instance.HealthHandler.healthScriptableObject.Death += OnPlayerKilled;
              
            }
        }

        public void ChangePlayerState(PlayerStateInfo newStateInfo)
        {
            playerState = newStateInfo.State;
            switch (playerState)
            {
                case PlayerState.Undetected:
                    HandleUndetectedMode();
                    break;
                case PlayerState.InCombat:
                    HandleDetectedMode(newStateInfo);
                    break;
                case PlayerState.Hiding:
                    HandleHideableMode(newStateInfo);
                    break;
                case PlayerState.Dead:
                    HandleDeadState(newStateInfo);
                    break;
                default:
                    Debug.LogError("Error changing player state: (State) " + newStateInfo.State);
                    break;
            }

            PlayerStateChangedEvent?.Invoke(newStateInfo);
        }

        private void HandleDeadState(PlayerStateInfo newStateInfo)
        {
            PlayerMovement.enabled = false;
            PlayerCombat.meleeHandler.enabled = false;
            PlayerCombat.enabled = false;
            CameraEffects.enabled = false;
            PlayerBlocking.enabled = false;
            PlayerLooking.EnterUndetectedFirstPersonMode();
            PlayerLooking.enabled = false;

        }

        private void HandleDetectedMode(PlayerStateInfo newStateInfo)
        {
           
            PlayerMovement.enabled = false;
            PlayerCombat.enabled = false;
            CameraEffects.enabled = false;
            PlayerCombat.meleeHandler.enabled = true;
            PlayerBlocking.enabled = true;
            PlayerLooking.enabled = true;
            PlayerLooking.EnterDetectedFirstPersonMode(newStateInfo.CameraRotationTarget);
        }

        private void HandleUndetectedMode()
        {
            PlayerMovement.enabled = true;
            PlayerCombat.enabled = true;
            CameraEffects.enabled = true;
            PlayerCombat.meleeHandler.enabled = true;
            PlayerBlocking.enabled = false;
            PlayerLooking.enabled = true;
            PlayerLooking.EnterUndetectedFirstPersonMode();
            
        }

        private void HandleHideableMode(PlayerStateInfo newStateInfo)
        {
            CameraEffects.enabled = false;
            PlayerCombat.enabled = false;
            PlayerCombat.meleeHandler.enabled = false;
            PlayerLooking.enabled = true;
            PlayerLooking.EnterHiddenInObjectMode(newStateInfo.CameraRotationTarget, newStateInfo.CameraPosition);
            PlayerBlocking.enabled = false;
            PlayerMovement.enabled = false;
        }

        private void OnPlayerKilled()
        {
            Instance.PlayerLooking.enabled = false;
            Instance.PlayerMovement.enabled = false;
            Instance.PlayerCombat.enabled = false;
            Instance.CameraEffects.enabled = false;
        }
    }
}
