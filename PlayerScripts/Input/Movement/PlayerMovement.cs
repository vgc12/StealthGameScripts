using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts.Input.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        #region Variables

        [Header("Shared")]
        public bool MovementEnabled { get; set; }
        public PlayerInputActions PlayerInputActions { get; private set; }
        private Transform _player;
  

        [Header("Movement")]
        [SerializeField] private CharacterController controller;

        
        public float Speed
        {
            get => speed;
            private set => speed = value;
        }
        [SerializeField] private float speed = 5f;

        public Action<float> SpeedChangedEvent;
        
        public float CrouchMovementSpeed
        {
            get => crouchMovementSpeed;
            private set => crouchMovementSpeed = value;
        }
        
        [SerializeField]
        private float crouchMovementSpeed = 3f;
        [SerializeField]
        private float speedMultiplier = 10f;
        [SerializeField]
        private Transform orientation;
    
        private Vector3 _previousPosition;
        private Vector3 _moveDirection;
        private Vector2 _movementInput;
        private Vector3 _velocityY;
        [SerializeField] private float gravity;
        private float _realSpeed;
        [SerializeField]
        private float acceleration = 1f;
        [SerializeField]
        private float deceleration = 1f;

        private Vector3 _lastMoveDirection;



        [Header("Jumping")]
        [SerializeField]
        private float maxJumpTime = 0.4f;
        [SerializeField]
        private float jumpVelocity = 5f;
        [SerializeField]
        private float maxJumpHeight = 5f;
        private bool _jumpPressed, _isJumping;

        [Header("Ladder Climbing")]
        [Range(0f, 10f)]
        [SerializeField] private float ladderSpeed = 5f;
        public bool isOnLadder;

        public Action<bool> LadderEvent;


        [Header("Crouching")]
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float normalHeight = 1f;
        [SerializeField] private float crouchSpeed = .5f;
        private float _playerHeight = 1f;
        private bool objectAbove = false;

        private bool IsCrouched{ get; set; }

        private bool _isCrouched;

        public delegate void CrouchDelegate(bool isCrouched);
        public CrouchDelegate CrouchEvent;
        private readonly Collider[] _crouchCollider = new Collider[1];
        private bool _crouchTransitioning;

    
        [Header("Leaning")]
        [SerializeField] private Transform leanPivot;
        [SerializeField] private Transform leanPivotCheck;
        [SerializeField] private Transform leanOverlapSphereLocation;
        [SerializeField] private float leanSpeed;
        [SerializeField]
        private float maxLeanAngle;
        private float _targetLean;
        private float _currentLean;
        private readonly Collider[] _leanCollider = new Collider[1];
        private float _leanVelocity;
        [SerializeField]
        private LayerMask leanLayerMask;

        private bool _leanLeftPressed, _leanRightPressed;
  

        #endregion
    
        #region UnityDefaults
        private void Awake()
        {
            _player = transform;
            _playerHeight = _player.localScale.y;
        
            PlayerInputActions = new PlayerInputActions();
            PlayerInputActions.Player.Enable();
        
            PlayerInputActions.Player.Jump.started += Jump;
            PlayerInputActions.Player.Jump.performed += Jump;
            PlayerInputActions.Player.Jump.canceled += Jump;
        
            PlayerInputActions.Player.Crouch.performed += ToggleCrouch;
        
            PlayerInputActions.Player.Move.performed += ctx => _movementInput = ctx.ReadValue<Vector2>();
            PlayerInputActions.Player.Move.canceled += _ => _movementInput = Vector2.zero;
        
            PlayerInputActions.Player.LeanLeft.performed += LeanLeft;
            PlayerInputActions.Player.LeanLeft.canceled += LeanLeft;
            PlayerInputActions.Player.LeanLeft.started += LeanLeft;
        
            PlayerInputActions.Player.LeanRight.performed += LeanRight;
            PlayerInputActions.Player.LeanRight.canceled += LeanRight;
            PlayerInputActions.Player.LeanRight.started += LeanRight;
            
            //Enemy.CombatStateChangedEvent += (enemy, inCombat) => MovementEnabled = !inCombat;
            MovementEnabled = true;
            speed *= speedMultiplier;
            
            SetUpGravity();
        }

        private void OnEnable()
        {
            PlayerInputActions.Player.Enable();
        }

        private void Update()
        {
            if(!MovementEnabled) return;
            if (isOnLadder)
            {
                MovePlayerOnLadder();
            }
            else
            {
                Crouch();
                MovePlayer();
                HandleGravity();
                HandleLean();
            }
            HandleJump();

        }

        


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawSphere(leanOverlapSphereLocation.position, .5f);
        }
        #endregion

        #region Falling
        private void SetUpGravity()
        {
            float timeToApex = maxJumpTime / 2;
            gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
            jumpVelocity = (2 * maxJumpHeight) / timeToApex;
        
        }

    
        private void HandleGravity()
        {
        
            if (controller.isGrounded && !_crouchTransitioning)
            {
                _velocityY.y = -2f;
           
            
            }
            else
            {
                float prevYVelocity = _velocityY.y;
                _velocityY.y += (gravity * Time.deltaTime);
                float nextYVelocity = (_velocityY.y + prevYVelocity) * .5f;
                _velocityY.y = nextYVelocity;
            

            }
        }

        #endregion

        #region Leaning
        private void LeanRight(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.started)
            {
                _leanLeftPressed = false;
                _leanRightPressed = true;
            }
            else
            {
                _leanRightPressed = false;
            }
        }

        private void LeanLeft(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.started)
            {
                _leanRightPressed = false;
                _leanLeftPressed = true;
            }
            else
            {
                _leanLeftPressed = false;
            }
        }

    

        private void HandleLean()
        {
       
            if (_leanLeftPressed)
            {
                _targetLean = maxLeanAngle;
            }
            else if (_leanRightPressed)
            {
                _targetLean = -maxLeanAngle;
            }
            else
            {
                _targetLean = 0;
            }
            leanPivotCheck.localRotation = Quaternion.Euler(new Vector3(0,0,_targetLean));

            bool objectInWay = Physics.OverlapSphereNonAlloc(leanOverlapSphereLocation.position, .5f, _leanCollider,
                ~leanLayerMask)> 0;
            
            
           
            switch (objectInWay)
            {
                case true when _targetLean != 0:
                    _targetLean = 0;
                    break;
                case true:
                    return;
            }
            _currentLean = Mathf.SmoothDamp(_currentLean, _targetLean, ref _leanVelocity, leanSpeed);
           
            var targetRotation = Quaternion.Euler(new Vector3(0,0,_currentLean));

            leanPivot.localRotation = targetRotation;


        }
        #endregion

        #region Crouching

        private void ToggleCrouch(InputAction.CallbackContext context)
        {
            var playerPosition = _player.position;
            objectAbove = Physics.OverlapCapsuleNonAlloc(
                new Vector3(playerPosition.x, playerPosition.y + crouchHeight, playerPosition.z),
                new Vector3(playerPosition.x, playerPosition.y + normalHeight, playerPosition.z),
                0.5f, _crouchCollider, ~leanLayerMask) > 0;
        
            if (context.performed)
            {

                if (IsCrouched && !objectAbove)
                {
                    IsCrouched = false;
                }
                else
                {
                    IsCrouched = true;
                }
                CrouchEvent.Invoke(IsCrouched);
            }
        }

        private void Crouch()
        {
       
            if (IsCrouched)
            {
                if (_playerHeight > crouchHeight)
                {
                    _crouchTransitioning = true;
                    _playerHeight -= crouchSpeed * Time.deltaTime;
                }
                else
                {
                    _crouchTransitioning = false;
                    _playerHeight = crouchHeight;
                }
            
            }
            else
            {
                if (_playerHeight < normalHeight)
                {
                    _playerHeight += crouchSpeed * Time.deltaTime;
                    _crouchTransitioning = true;
                }
                else
                {
                    _crouchTransitioning = false;
                    _playerHeight = normalHeight;
                }
            }
            transform.localScale = new Vector3(1, _playerHeight, 1);
        }
    

        #endregion
    
        #region Movement

      
       
        private void MovePlayer()
        {
       
        
            if(_movementInput.x != 0 || _movementInput.y != 0)
            {
             
                
                float prevSpeed = _realSpeed;
                _realSpeed = Mathf.Clamp( _realSpeed + acceleration * Time.deltaTime *.5f, 0,IsCrouched ? crouchMovementSpeed : speed);
                float nextSpeed = (_realSpeed + prevSpeed) * .5f;
                SpeedChangedEvent?.Invoke(nextSpeed);
                controller.Move( _moveDirection.normalized * (nextSpeed * Time.deltaTime));
                
           
                _moveDirection = (orientation.forward * _movementInput.y) + (orientation.right * _movementInput.x);
            
          
            }
            else
            {
                var prevSpeed = _realSpeed;
                if(!_isJumping)
                    _realSpeed = Mathf.Clamp( _realSpeed - deceleration * Time.deltaTime, 0, IsCrouched ? crouchMovementSpeed : speed);
                var nextSpeed = (_realSpeed + prevSpeed) * .5f;
                SpeedChangedEvent?.Invoke(nextSpeed);
            
                controller.Move( _lastMoveDirection.normalized * (nextSpeed * Time.deltaTime));
            }
        
        
            _lastMoveDirection = _moveDirection;
        
            controller.Move(_velocityY * Time.deltaTime);
        }


        #endregion
    
        #region Ladder
        private void MovePlayerOnLadder() => controller.Move(ladderSpeed * _movementInput.y * Time.deltaTime * Vector3.up);

        public void EnterLadder()
        {
            
            isOnLadder = true;
            LadderEvent.Invoke(isOnLadder);
        }

        public void ExitLadder()
        {
            isOnLadder = false;
            LadderEvent.Invoke(isOnLadder);
        } 

    

        #endregion
    
        #region Jumping
        private void Jump(InputAction.CallbackContext ctx)
        {

            _jumpPressed = ctx.ReadValueAsButton();
        }
     

        private void HandleJump()
        {
        
            if ((!_isJumping && controller.isGrounded && _jumpPressed) || 
                (isOnLadder && _jumpPressed))
            {

                if (isOnLadder)
                {
                    ExitLadder();
                }
                _velocityY.y = jumpVelocity;
                _isJumping = true;
            }
            else if (!_jumpPressed && _isJumping && controller.isGrounded)
            {
                _isJumping = false;
            }
        }

    

        #endregion

        public void DisableMovementInput() => PlayerInputActions.Player.Disable();


    }
}
