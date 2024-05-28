using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts
{
    public class CameraEffects : MonoBehaviour
    {
        private bool _isWalking;
        [Range(0,.1f)]
        [SerializeField] 
        private float standingHeadBobAmplitude;
        [Range(0,.1f)]
        [SerializeField]
        private float crouchHeadBobAmplitude;
        
        private float _variableHeadBobAmplitude;
        [Range(0,30)]
        [SerializeField] private float standingHeadBobFrequency;
        [Range(0,30)]
        [SerializeField] private float crouchHeadBobFrequency;
  
        private Vector3 _position;
        private Vector3 _startPos;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform cameraTransform;
        private bool _isCrouched;
        private float _playerSpeed;
        private bool _isOnLadder;
        private bool _effectsEnabled;
        public void Start()
        {
            Player.Instance.PlayerMovement.PlayerInputActions.Player.Move.performed += OnMove;
            Player.Instance.PlayerMovement.CrouchEvent += OnCrouch;
            Player.Instance.PlayerMovement.SpeedChangedEvent += OnSpeedChanged;
            Player.Instance.PlayerMovement.LadderEvent += OnLadderStateChanged;
            Player.Instance.PlayerStateChangedEvent += OnPlayerStateChanged;
            
        
            _effectsEnabled = true;
            _startPos = cameraTransform.localPosition;
        }

        private void OnPlayerStateChanged(PlayerStateInfo info) => _effectsEnabled = info.State == PlayerState.Undetected;

        private void OnLadderStateChanged(bool ladderState) => _isOnLadder = ladderState;

        private void OnCrouch (bool crouchedState) => _isCrouched = crouchedState;

        private void OnMove(InputAction.CallbackContext ctx) => _isWalking = ctx.performed;

        private void OnSpeedChanged(float speed) => _playerSpeed = speed;


   

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(_position, .3f);
        }

        private void Update()
        {
            
            if(!_effectsEnabled) return;
            ResetPosition();
            if(_isOnLadder) return;
            
            ApplyHeadBob(CalculateHeadBob());
            

          
        }

        private void ApplyHeadBob(Vector3 motion)
        {
            if (_playerSpeed > 0)
            {
                cameraTransform.localPosition += motion;
            }
        }

        private Vector3 CalculateHeadBob()
        {
            _variableHeadBobAmplitude = _isCrouched? crouchHeadBobAmplitude : standingHeadBobAmplitude;

            var t = (_isCrouched? crouchHeadBobFrequency :  standingHeadBobFrequency)  * Time.time;
            var x = Mathf.Cos(t/2) * _variableHeadBobAmplitude*2;
            var y = Mathf.Sin(t) * _variableHeadBobAmplitude;
            return new Vector3(x, y, 0);
        }
        
        private void ResetPosition()
        {
            if (cameraTransform.localPosition == _startPos)
            {
                return;
            }
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, _startPos, Time.deltaTime);
        }

        private Vector3 FocusTarget()
        {
            var playerPosition = transform.position;
            Vector3 pos = new(playerPosition.x, playerPosition.y + cameraHolder.localPosition.y, playerPosition.z);
            pos += cameraHolder.forward * 15f;
            return pos;
        }
        
        private void OnDestroy()
        {
            Player.Instance.PlayerMovement.PlayerInputActions.Player.Move.performed -= OnMove;
            Player.Instance.PlayerMovement.CrouchEvent -= OnCrouch;
            Player.Instance.PlayerMovement.SpeedChangedEvent -= OnSpeedChanged;
            Player.Instance.PlayerMovement.LadderEvent -= OnLadderStateChanged;
            Player.Instance.PlayerStateChangedEvent -= OnPlayerStateChanged;
        }
        
    }
}
