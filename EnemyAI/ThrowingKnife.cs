using UnityEngine;

namespace EnemyAI
{
    public class ThrowingKnife : MonoBehaviour
    {
        public Transform target;
        public float projectileSpeed = .06f;
        private float t = 0;
        public bool readyToThrow;
        private Collider[] hitColliders = new Collider[1];
        private Rigidbody rb;
        [SerializeField]
        private GameObject blockEffect;
    
    
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            readyToThrow = false;
        }

        private void Update()
        {
        
            if(!readyToThrow) return;
        
      
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * projectileSpeed);

            if (Physics.OverlapSphereNonAlloc(transform.position, .1f, hitColliders, LayerMask.GetMask("Weapon")) > 0)
            {
                rb.isKinematic = false;
                rb.AddForce((transform.position-target.position) *10, ForceMode.Impulse);
                Instantiate(blockEffect, transform.position, Quaternion.identity);
                //EnemyWeaponHandler.ThrowingKnifePool.Release(this.gameObject);


            }
        
            if (Vector3.Distance(transform.position, target.position) < .01f) {
                PlayerScripts.Player.Instance.HealthHandler.healthScriptableObject.TakeDamage(100);
                EnemyWeaponHandler.ThrowingKnifePool.Release(this.gameObject);
            }
        
        }

  
    }
}