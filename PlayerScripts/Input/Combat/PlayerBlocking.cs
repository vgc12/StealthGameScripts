using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlayerScripts.Input.Combat
{
    public class PlayerBlocking : MonoBehaviour
    {
        private PlayerInputActions _playerInputActions;
        private Vector2 _keyboardDelta;
        private Vector2 _objectPosition;
        [SerializeField]
        private Transform weaponTransform;
        [SerializeField]
        private float rotationTime = 0.01f;
        private Vector3 _direction;
        private bool _isRunning;
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.Enable();
            // playerInputActions.Player.Move.performed += ctx => keyboardDelta = ctx.ReadValue<Vector2>();
            _playerInputActions.Player.Move.performed += MoveSword;
            _playerInputActions.Player.Move.canceled += ctx => _keyboardDelta = Vector2.zero;
        }

        private void MoveSword(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && enabled)
            {
                _keyboardDelta = ctx.ReadValue<Vector2>();
                if (_isRunning || !enabled) return;
                StartCoroutine(MoveSwordToPosition());
                _isRunning = true;
            }
        }

        private IEnumerator MoveSwordToPosition()
        {
            var startRotation = weaponTransform.localRotation;
            var zRotation = (MathF.Atan2(_keyboardDelta.y, _keyboardDelta.x) * Mathf.Rad2Deg);
      
            var finalRotation = Quaternion.Euler(new Vector3( weaponTransform.localRotation.x, weaponTransform.localRotation.y, zRotation));
            var t = 0f;
            while (t <= 1f)
            {
                t += Time.deltaTime / rotationTime;
                weaponTransform.localRotation = Quaternion.Lerp(startRotation, finalRotation, t);
                yield return null;
            }
            _isRunning = false;
        }
    
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, _direction);
        }

        private void OnDisable()
        {
            weaponTransform.localRotation = Quaternion.Euler( new Vector3(weaponTransform.localRotation.x, weaponTransform.localRotation.y, 0));
        }
    }
}
