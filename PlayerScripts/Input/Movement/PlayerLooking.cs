using System.Collections;
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
        private PlayerInputActions _playerInputActions;

        [SerializeField] 
        private Transform orientation;

        private Transform _rotationTarget;
    
        private Vector2 _mouseDelta;

        public float sensX = 100f;
        public float sensY = 100f;

        [SerializeField]
        private float minRotationClamp = -89f;
        [SerializeField]
        private float maxRotationClamp = 89f;
    
        private float _xRotation;
        private float _yRotation;

        private bool _freeCursor = false;

        private CameraState _cameraState;

        [SerializeField] private HealthHandler playerHealthHandler;
        [SerializeField] private Camera weaponCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField]
        private Transform cameraPosition;
        private Transform _playerCameraPosition;
        private GameObject _player;
        

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _cameraState = CameraState.UndetectedFirstPerson;
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Look.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Look.canceled += ctx => _mouseDelta = Vector2.zero;
            
            playerHealthHandler.healthScriptableObject.Death += OnDeath;
            _rotationTarget = transform;
            _playerCameraPosition = cameraPosition;
            _player = Player.Instance.gameObject;
            mainCamera.fieldOfView = 90;
            weaponCamera.fieldOfView = 90;
            _freeCursor = false;
        }



        // Update is called once per frame
        private void Update()
        {
            transform.position = cameraPosition.position;
            if (_freeCursor) return;
            
            if (_cameraState != CameraState.UndetectedFirstPerson)
            {
                transform.LookAt(_rotationTarget);
                    
            }

            if (_cameraState == CameraState.DetectedFirstPerson)
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
            var mouseX = _mouseDelta.x * Time.deltaTime * sensX;
            var mouseY = _mouseDelta.y * Time.deltaTime * sensY;

            _yRotation += mouseX;
            _xRotation -= mouseY;

            _xRotation = Mathf.Clamp(_xRotation, minRotationClamp, maxRotationClamp);
        
            _rotationTarget.rotation = Quaternion.Euler(_xRotation, _yRotation, 0);
        
            orientation.rotation = Quaternion.Euler(0, _yRotation, 0);
       
        }

        private bool running;

        public void EnterDetectedFirstPersonMode(Transform rotTarget)
        {
            _cameraState = CameraState.DetectedFirstPerson;
            _rotationTarget = rotTarget;
            if(running) return;
            running = true;
            StartCoroutine(LerpCameraFov(60f));
        }
        public void EnterUndetectedFirstPersonMode()
        {
            _cameraState = CameraState.UndetectedFirstPerson;
            _rotationTarget = transform;
            cameraPosition = _playerCameraPosition;
           
            minRotationClamp = -90f;
            maxRotationClamp = 90f;
            weaponCamera.enabled = true;
            
            StartCoroutine(LerpCameraFov(90f));
        }
    
        public void EnterHiddenInObjectMode(Transform rotTarget, Transform camPosition)
        {
            _rotationTarget = rotTarget;
            cameraPosition = camPosition;
            _cameraState = CameraState.HiddenInObject;
          
            minRotationClamp = 0;
            maxRotationClamp = 89f;
            weaponCamera.enabled = false;
        }

        private IEnumerator LerpCameraFov(float toFov)
        {
            while (!Mathf.Approximately(mainCamera.fieldOfView, toFov))
            {
                mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, toFov, Time.deltaTime * 2);
                weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, toFov, Time.deltaTime * 2);
                
                yield return null;
            }

            running = false;
        }
        
    
        private void OnDeath()
        {
            Cursor.lockState = CursorLockMode.Confined;
            weaponCamera.enabled = false;
            Cursor.visible = true;
            _freeCursor = true;
        }
    }
}