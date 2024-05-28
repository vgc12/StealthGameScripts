using System.Collections;
using Damageables;
using UnityEngine;
using UnityEngine.Events;

namespace Weapons.Melee
{
    [CreateAssetMenu(fileName = "Ranged Weapon Scriptable Object", menuName = "Weapons/Ranged Weapon", order = 0)]
    public class RangedWeaponScriptableObject : WeaponScriptableObject
    {
        private Rigidbody _rb;
        private Collider _weaponCol;
        [SerializeField] public float throwSpeed = 30f;
        public bool Thrown
        {
            get => _thrown;
            set
            {
                _weaponCol.enabled = value;
                _thrown = value;
                if (value)
                {
                    Model.SetActive(true);
                    Model.transform.SetParent(null);
                    _weaponCol.enabled = true;
                }
                else
                {
                    _rb.isKinematic = true;
                    _weaponCol.enabled = false;
                    Model.transform.SetParent(weaponParent);
                    ResetWeaponLocation();
                }
            }
        }

        private bool _thrown;


        public override void Spawn(Transform parent, MonoBehaviour monoBehavior)
        {
            base.Spawn(parent, monoBehavior);
            _rb = Model.AddComponent<Rigidbody>();
            _rb.isKinematic = true;
            _weaponCol = Model.GetComponent<Collider>();
            Thrown = false;

        }

        protected override void PlayAttackAnimation()
        {
            WeaponAnimator.SetTrigger(attackAnimationName);
        }
        
        public override void Attack(RaycastHit hit)
        {
            if (Thrown)
            {
                return;
            }

           
            Thrown = true;
            ActiveMonoBehavior.StartCoroutine(ThrowCoroutine(hit.point));
            
        }

        private IEnumerator ThrowCoroutine(Vector3 hitPos)
        {
           
            while (Vector3.Distance(Model.transform.position, hitPos) > .1f )
            {
                Model.transform.position = Vector3.MoveTowards(Model.transform.position, hitPos, throwSpeed * Time.deltaTime);
               
                var col = new Collider[1];
                if (Physics.OverlapSphereNonAlloc(Model.transform.position, .5f, col, LayerMask.GetMask("Hittable")) > 0)
                {
                    TryDamage(col[0]);
                    _rb.isKinematic = false;
                }

                _rb.isKinematic = false;
                yield return null;
            }

            ActiveMonoBehavior.StartCoroutine(AllowPickUpCoroutine());
            yield return null;
        }
     
        
        private IEnumerator AllowPickUpCoroutine()
        {
            var overlapCol = new Collider[1];
            while (Thrown)
            {
                if (Physics.OverlapSphereNonAlloc(Model.transform.position, .5f, overlapCol,
                        LayerMask.GetMask("Player")) > 0)
                {
                    Thrown = false;
                    PickedUp.Invoke(this);
                }

                yield return null;
            }
        }
    }
    
}
