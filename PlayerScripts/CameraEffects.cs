using UnityEngine;

namespace PlayerScripts
{
    public class CameraEffects : MonoBehaviour
    {
        private bool isWalking;
        [Range(0,.1f)]
        [SerializeField] 
        private float standingHeadBobAmplitude;
        [Range(0,.1f)]
        [SerializeField]
        private float crouchHeadBobAmplitude;
        
        private float variableHeadBobAmplitude;
        [Range(0,30)]
        [SerializeField] private float standingHeadBobFrequency;
        [Range(0,30)]
        [SerializeField] private float crouchHeadBobFrequency;
  
        private Vector3 position;
        private Vector3 startPos;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private Transform cameraTransform;
        private bool isCrouched;
        private float playerSpeed;
        private float maxStandingSpeed;
        private float maxCrouchedSpeed;
        private bool isOnLadder;
        private bool effectsEnabled;
        public void Start()
        {
            Player.Instance.PlayerMovement.PlayerInputActions.Player.Move.started += ctx => isWalking = true;
            Player.Instance.PlayerMovement.PlayerInputActions.Player.Move.canceled += ctx => isWalking = false;
            Player.Instance.PlayerMovement.CrouchEvent += crouchedState => isCrouched = crouchedState;
            Player.Instance.PlayerMovement.SpeedChangedEvent += speed => playerSpeed = speed;
            maxStandingSpeed = Player.Instance.PlayerMovement.Speed;
            maxCrouchedSpeed = Player.Instance.PlayerMovement.CrouchMovementSpeed;
            Player.Instance.PlayerMovement.LadderEvent += ladderState => isOnLadder = ladderState;
            Player.PlayerStateChangedEvent += info =>  effectsEnabled = info.State == PlayerState.Undetected;
            effectsEnabled = true;
            startPos = cameraTransform.localPosition;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(position, .3f);
        }

        private void Update()
        {
            
            if(!effectsEnabled) return;
            ResetPosition();
            if(isOnLadder) return;
            
            ApplyHeadBob(CalculateHeadBob());
            

          
        }

        private void ApplyHeadBob(Vector3 motion)
        {
            if (playerSpeed > 0)
            {
                cameraTransform.localPosition += motion;
            }
        }

        private Vector3 CalculateHeadBob()
        {
            var lerpTime = isCrouched ? playerSpeed / maxCrouchedSpeed : playerSpeed / maxStandingSpeed;
            
            variableHeadBobAmplitude = isCrouched? crouchHeadBobAmplitude : standingHeadBobAmplitude;

            //Debug.Log(variableHeadBobAmplitude + "variable amplitude");
           //Debug.Log(headBobAmplitude + "head bob amplitude");
            var t = (isCrouched? crouchHeadBobFrequency :  standingHeadBobFrequency)  * Time.time;
            var x = Mathf.Cos(t/2) * variableHeadBobAmplitude*2;
            var y = Mathf.Sin(t) * variableHeadBobAmplitude;
            return new Vector3(x, y, 0);
        }
        
        private void ResetPosition()
        {
            if (cameraTransform.localPosition == startPos)
            {
                return;
            }
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, startPos, Time.deltaTime);
        }

        private Vector3 FocusTarget()
        {
            var playerPosition = transform.position;
            Vector3 pos = new(playerPosition.x, playerPosition.y + cameraHolder.localPosition.y, playerPosition.z);
            pos += cameraHolder.forward * 15f;
            return pos;
        }
        
    }
}
