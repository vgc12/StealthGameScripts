using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBlocking : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    private Vector2 keyboardDelta;
    private Vector2 objectPosition;
    [SerializeField]
    private Transform weaponTransform;
    [SerializeField]
    private float rotationTime = 0.01f;
    private Vector3 direction;
    private bool isRunning;
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
       // playerInputActions.Player.Move.performed += ctx => keyboardDelta = ctx.ReadValue<Vector2>();
       playerInputActions.Player.Move.performed += MoveSword;
       playerInputActions.Player.Move.canceled += ctx => keyboardDelta = Vector2.zero;
    }

    private void MoveSword(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && enabled)
        {
            keyboardDelta = ctx.ReadValue<Vector2>();
            if (isRunning) return;
            StartCoroutine(MoveSwordToPosition());
            isRunning = true;
        }
    }

    private IEnumerator MoveSwordToPosition()
    {
        var startRotation = weaponTransform.localRotation;
        var zRotation = (MathF.Atan2(keyboardDelta.y, keyboardDelta.x) * Mathf.Rad2Deg);
      
        var finalRotation = Quaternion.Euler(new Vector3( weaponTransform.localRotation.x, weaponTransform.localRotation.y, zRotation));
        var t = 0f;
        while (t <= 1f)
        {
            t += Time.deltaTime / rotationTime;
            weaponTransform.localRotation = Quaternion.Lerp(startRotation, finalRotation, t);
            yield return null;
        }
        isRunning = false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, direction);
    }

   
    
    private void Update()
    {
      
        
        //var zRotation = (MathF.Atan2(keyboardDelta.y, keyboardDelta.x) * Mathf.Rad2Deg);
        //weaponTransform.localRotation = Quaternion.Euler(new Vector3( weaponTransform.localRotation.x, weaponTransform.localRotation.y, zRotation));
        //weaponTransform.localRotation = Quaternion.LookRotation()
        //weaponTransform.rotation = Quaternion.Euler( );

    }
}
