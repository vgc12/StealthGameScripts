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
        private Transform player;
  

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
    
        private Vector3 previousPosition;
        private Vector3 moveDirection;
        private Vector2 movementInput;
        private Vector3 velocityY;
        [SerializeField] private float gravity;
        private float realSpeed;
        [SerializeField]
        private float acceleration = 1f;
        [SerializeField]
        private float deceleration = 1f;

        private Vector3 lastMoveDirection;



        [Header("Jumping")]
        [SerializeField]
        private float maxJumpTime = 0.4f;
        [SerializeField]
        private float jumpVelocity = 5f;
        [SerializeField]
        private float maxJumpHeight = 5f;
        private bool jumpPressed, isJumping;

        [Header("Ladder Climbing")]
        [Range(0f, 10f)]
        [SerializeField] private float ladderSpeed = 5f;
        public bool isOnLadder;

        public Action<bool> LadderEvent;


        [Header("Crouching")]
        [SerializeField] private float crouchHeight = 0.5f;
        [SerializeField] private float normalHeight = 1f;
        [SerializeField] private float crouchSpeed = .5f;
        private float playerHeight = 1f;
        [SerializeField]
        private bool isCrouched = false;
        public delegate void CrouchDelegate(bool isCrouched);
        public CrouchDelegate CrouchEvent;
        private readonly Collider[] crouchCollider = new Collider[1];
        private bool crouchTransitioning;

    
        [Header("Leaning")]
        [SerializeField] private Transform leanPivot;
        [SerializeField] private Transform leanPivotCheck;
        [SerializeField] private Transform leanOverlapSphereLocation;
        [SerializeField] private float leanSpeed;
        [SerializeField]
        private float maxLeanAngle;
        private float targetLean;
        private float currentLean;
        private readonly Collider[] leanCollider = new Collider[1];
        private float leanVelocity;
        [SerializeField]
        private LayerMask leanLayerMask;

        private bool leanLeftPressed, leanRightPressed;
  

        #endregion
    
        #region UnityDefaults
        private void Awake()
        {
            player = transform;
            playerHeight = player.localScale.y;
        
            PlayerInputActions = new PlayerInputActions();
            PlayerInputActions.Player.Enable();
        
            PlayerInputActions.Player.Jump.started += Jump;
            PlayerInputActions.Player.Jump.performed += Jump;
            PlayerInputActions.Player.Jump.canceled += Jump;
        
            PlayerInputActions.Player.Crouch.performed += ToggleCrouch;
        
            PlayerInputActions.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
            PlayerInputActions.Player.Move.canceled += _ => movementInput = Vector2.zero;
        
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
        
            if (controller.isGrounded && !crouchTransitioning)
            {
                velocityY.y = -2f;
           
            
            }
            else
            {
                float prevYVelocity = velocityY.y;
                velocityY.y += (gravity * Time.deltaTime);
                float nextYVelocity = (velocityY.y + prevYVelocity) * .5f;
                velocityY.y = nextYVelocity;
            

            }
        }

        #endregion

        #region Leaning
        private void LeanRight(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.started)
            {
                leanLeftPressed = false;
                leanRightPressed = true;
            }
            else
            {
                leanRightPressed = false;
            }
        }

        private void LeanLeft(InputAction.CallbackContext obj)
        {
            if (obj.performed || obj.started)
            {
                leanRightPressed = false;
                leanLeftPressed = true;
            }
            else
            {
                leanLeftPressed = false;
            }
        }

    

        private void HandleLean()
        {
       
            if (leanLeftPressed)
            {
                targetLean = maxLeanAngle;
            }
            else if (leanRightPressed)
            {
                targetLean = -maxLeanAngle;
            }
            else
            {
                targetLean = 0;
            }
            leanPivotCheck.localRotation = Quaternion.Euler(new Vector3(0,0,targetLean));

            bool objectInWay = Physics.OverlapSphereNonAlloc(leanOverlapSphereLocation.position, .5f, leanCollider,
                ~leanLayerMask)> 0;
            
            
           
            switch (objectInWay)
            {
                case true when targetLean != 0:
                    targetLean = 0;
                    break;
                case true:
                    return;
            }
            currentLean = Mathf.SmoothDamp(currentLean, targetLean, ref leanVelocity, leanSpeed);
           
            var targetRotation = Quaternion.Euler(new Vector3(0,0,currentLean));

            leanPivot.localRotation = targetRotation;


        }
        #endregion

        #region Crouching

        private void ToggleCrouch(InputAction.CallbackContext context)
        {
            var playerPosition = player.position;
            bool objectAbove = Physics.OverlapCapsuleNonAlloc(
                new Vector3(playerPosition.x, playerPosition.y + crouchHeight, playerPosition.z),
                new Vector3(playerPosition.x, playerPosition.y + normalHeight, playerPosition.z),
                0.5f, crouchCollider, ~leanLayerMask) > 0;
        
            if (context.performed)
            {

                if (isCrouched && !objectAbove)
                {
                    isCrouched = false;
                }
                else
                {
                    isCrouched = true;
                }
                CrouchEvent.Invoke(isCrouched);
            }
        }

        private void Crouch()
        {
       
            if (isCrouched)
            {
                if (playerHeight > crouchHeight)
                {
                    crouchTransitioning = true;
                    playerHeight -= crouchSpeed * Time.deltaTime;
                }
                else
                {
                    crouchTransitioning = false;
                    playerHeight = crouchHeight;
                }
            
            }
            else
            {
                if (playerHeight < normalHeight)
                {
                    playerHeight += crouchSpeed * Time.deltaTime;
                    crouchTransitioning = true;
                }
                else
                {
                    crouchTransitioning = false;
                    playerHeight = normalHeight;
                }
            }
            transform.localScale = new Vector3(1, playerHeight, 1);
        }
    

        #endregion
    
        #region Movement

      
       
        private void MovePlayer()
        {
       
        
            if(movementInput.x != 0 || movementInput.y != 0)
            {
             
                
                float prevSpeed = realSpeed;
                realSpeed = Mathf.Clamp( realSpeed + acceleration * Time.deltaTime *.5f, 0,isCrouched ? crouchMovementSpeed : speed);
                float nextSpeed = (realSpeed + prevSpeed) * .5f;
                SpeedChangedEvent?.Invoke(nextSpeed);
                controller.Move( moveDirection.normalized * (nextSpeed * Time.deltaTime));
                
           
                moveDirection = (orientation.forward * movementInput.y) + (orientation.right * movementInput.x);
            
          
            }
            else
            {
                var prevSpeed = realSpeed;
                if(!isJumping)
                    realSpeed = Mathf.Clamp( realSpeed - deceleration * Time.deltaTime, 0, isCrouched ? crouchMovementSpeed : speed);
                var nextSpeed = (realSpeed + prevSpeed) * .5f;
                SpeedChangedEvent?.Invoke(nextSpeed);
            
                controller.Move( lastMoveDirection.normalized * (nextSpeed * Time.deltaTime));
            }
        
        
            lastMoveDirection = moveDirection;
        
            controller.Move(velocityY * Time.deltaTime);
        }


        #endregion
    
        #region Ladder
        private void MovePlayerOnLadder() => controller.Move(ladderSpeed * movementInput.y * Time.deltaTime * Vector3.up);

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

            jumpPressed = ctx.ReadValueAsButton();
        }
     

        private void HandleJump()
        {
        
            if ((!isJumping && controller.isGrounded && jumpPressed) || 
                (isOnLadder && jumpPressed))
            {

                if (isOnLadder)
                {
                    ExitLadder();
                }
                velocityY.y = jumpVelocity;
                isJumping = true;
            }
            else if (!jumpPressed && isJumping && controller.isGrounded)
            {
                isJumping = false;
            }
        }

    

        #endregion

        public void DisableMovementInput() => PlayerInputActions.Player.Disable();


    }
}
