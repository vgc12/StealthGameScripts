using Damageables;
using UnityEngine;

namespace PlayerScripts.Input.Movement
{
    public enum CameraState
    {
        UndetectedFirstPerson,
        DetectedFirstPerson,
        HiddenInObject
    }

    public class PlayerLooking : MonoBehaviour
    {
        private PlayerInputActions playerInputActions;

        [SerializeField] 
        private Transform orientation;

        private Transform rotationTarget;
    
        private Vector2 mouseDelta;

        public float sensX = 100f;
        public float sensY = 100f;

        [SerializeField]
        private float minRotationClamp = -89f;
        [SerializeField]
        private float maxRotationClamp = 89f;
    
        private float xRotation;
        private float yRotation;

        private bool freeCursor = false;

        private CameraState cameraState;

        [SerializeField] private HealthHandler playerHealthHandler;
        [SerializeField] private Camera weaponCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField]
        private Transform cameraPosition;
        private Transform playerCameraPosition;
        private GameObject player;
        

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cameraState = CameraState.UndetectedFirstPerson;
            playerInputActions = new PlayerInputActions();
            playerInputActions.Player.Enable();
            playerInputActions.Player.Look.performed += ctx => mouseDelta = ctx.ReadValue<Vector2>();
            playerInputActions.Player.Look.canceled += ctx => mouseDelta = Vector2.zero;
            
            playerHealthHandler.healthScriptableObject.Death += OnDeath;
            rotationTarget = transform;
            playerCameraPosition = cameraPosition;
            player = Player.Instance.gameObject;
            mainCamera.fieldOfView = 90;
            weaponCamera.fieldOfView = 90;
            freeCursor = false;
        }



        // Update is called once per frame
        private void Update()
        {
            transform.position = cameraPosition.position;
            if (freeCursor) return;
            
            if (cameraState != CameraState.UndetectedFirstPerson)
            {
                transform.LookAt(rotationTarget);
                    
            }

            if (cameraState == CameraState.DetectedFirstPerson)
            {
                orientation.rotation = Quaternion.Euler(0, transform.rotation.y,0);
            }
            else
            {
                MoveCamera();
            }


        }
    
    

        private void MoveCamera()
        {
            var mouseX = mouseDelta.x * Time.deltaTime * sensX;
            var mouseY = mouseDelta.y * Time.deltaTime * sensY;

            yRotation += mouseX;
            xRotation -= mouseY;

            xRotation = Mathf.Clamp(xRotation, minRotationClamp, maxRotationClamp);
        
            rotationTarget.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);
       
        }


        public void EnterDetectedFirstPersonMode(Transform rotTarget)
        {
            cameraState = CameraState.DetectedFirstPerson;
            rotationTarget = rotTarget;
            LerpCameraFov(60f);
        }
        public void EnterUndetectedFirstPersonMode()
        {
            cameraState = CameraState.UndetectedFirstPerson;
            rotationTarget = transform;
            cameraPosition = playerCameraPosition;
           
            minRotationClamp = -90f;
            maxRotationClamp = 90f;
            weaponCamera.enabled = true;
            LerpCameraFov(90f);
        }
    
        public void EnterHiddenInObjectMode(Transform rotTarget, Transform camPosition)
        {
            rotationTarget = rotTarget;
            cameraPosition = camPosition;
            cameraState = CameraState.HiddenInObject;
          
            minRotationClamp = 0;
            maxRotationClamp = 89f;
            weaponCamera.enabled = false;
        }

        private void LerpCameraFov(float toFov)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, toFov, Time.deltaTime * 2);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, toFov, Time.deltaTime * 2);
        }
        
    
        private void OnDeath()
        {
            Cursor.lockState = CursorLockMode.Confined;
            weaponCamera.enabled = false;
            Cursor.visible = true;
            freeCursor = true;
        }
    }
}