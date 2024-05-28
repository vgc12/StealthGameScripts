using Damageables;
using PlayerScripts;
using UnityEngine;

namespace EnemyAI
{
    public class ThrowingKnife : MonoBehaviour
    {
        public Transform target;
        public float projectileSpeed = .06f;
     
        public bool readyToThrow;
        private readonly Collider[] _hitColliders = new Collider[1];
        private Rigidbody _rb;
        [SerializeField]
        private GameObject blockEffect;

        public bool lastKnife;
    
    
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            readyToThrow = false;
        }

        private void Update()
        {
            if(!readyToThrow ) return;
        
      
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime * projectileSpeed);

            if (Physics.OverlapSphereNonAlloc(transform.position, .1f, _hitColliders, LayerMask.GetMask("Weapon")) > 0 && Player.Instance.playerState != PlayerState.Dead)
            {
                _rb.isKinematic = false;
                _rb.AddForce((transform.position-target.position) *10, ForceMode.Impulse);
                Instantiate(blockEffect, transform.position, Quaternion.identity);
                //EnemyWeaponHandler.ThrowingKnifePool.Release(this.gameObject);


            }

            if (lastKnife)
            {
                if (Physics.OverlapSphereNonAlloc(transform.position, .04f, _hitColliders,
                        LayerMask.GetMask("Hittable") )> 0)
                {
                    _hitColliders[0].GetComponentInParent<HealthHandler>().healthScriptableObject.TakeDamage(100);
                }
            }
            
            if (Vector3.Distance(transform.position, target.position) < .01f) {
                PlayerScripts.Player.Instance.HealthHandler.healthScriptableObject.TakeDamage(100);
                EnemyWeaponHandler.ThrowingKnifePool.Release(gameObject);
            }
        
        }

  
    }
}