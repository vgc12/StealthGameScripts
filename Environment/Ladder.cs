using PlayerScripts.Input.Movement;
using UnityEngine;

namespace Environment
{
    public class Ladder : MonoBehaviour
    {
 
    
  

        private PlayerMovement playerMovement;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {

                playerMovement.EnterLadder();
            
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") )
            {

                playerMovement.ExitLadder();
            }
        }

        void Start()
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        // Update is called once per frame
        void Update()
        {

    
        }
    }
}
