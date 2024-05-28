using PlayerScripts;
using PlayerScripts.Input.Movement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Environment
{
    public class Hideable : MonoBehaviour
    {
        private PlayerInputActions playerInputActions;
        private PlayerLooking playerLooking;
    
        [SerializeField]
        private GameObject rotationTarget;
        [SerializeField]
        private GameObject cameraPosition;

        [SerializeField] private GameObject cameraCheck;
        [SerializeField]
        private float cameraDistance;
   
        private bool inHideable;
 
        
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                playerInputActions.UI.EnterHideable.Enable();
            
            }
        
    
        }
    
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                playerInputActions.UI.EnterHideable.Disable();
            
            }
        }

        private void Start()
        {
            playerInputActions = new PlayerInputActions();
            playerInputActions.UI.EnterHideable.performed += EnterHideable;
          
            playerLooking = Player.Instance.PlayerLooking;
            inHideable = false;
            cameraDistance = -cameraDistance;
        }

        private void OnDestroy()
        {
            playerInputActions.UI.EnterHideable.performed -= EnterHideable;
        }

        private void EnterHideable(InputAction.CallbackContext obj)
        {
           
            if (!inHideable)
            {
           
                //playerLooking.EnterHiddenInObjectMode(rotationTarget.transform, cameraPosition.transform);
                Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Hiding, rotationTarget.transform, cameraPosition.transform));

            }
            else
            {
                Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Undetected));
            }
            inHideable = !inHideable;
        }


        public void ForceExitHideable()
        {
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Undetected));
            inHideable = false;
        }

 
        private void Update()
        {
            if (!inHideable) return;
            if(Physics.Linecast(rotationTarget.transform.position, cameraCheck.transform.position, out var hit, ~LayerMask.GetMask("Hideable")))
            {
                
                
                cameraPosition.transform.position = hit.point;
                var localPosition = cameraPosition.transform.localPosition;
                localPosition = new Vector3(localPosition.x,
                    localPosition.y, localPosition.z + .2f);
                
                
                cameraPosition.transform.localPosition = localPosition;
               
                
            }
            else
            {
                cameraPosition.transform.localPosition = new Vector3(0, 0, cameraDistance);
                cameraCheck.transform.localPosition = new Vector3(0, 0, cameraDistance-.2f);
            }
        }
    }
}
