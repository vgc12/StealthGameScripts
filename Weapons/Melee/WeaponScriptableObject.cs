using System.Collections;
using Damageables;
using UnityEngine;
using UnityEngine.Events;

namespace Weapons.Melee
{
    public enum WeaponType
    {
        Ranged,
        Melee
    }
    
    public class WeaponScriptableObject : ScriptableObject
    {
        public WeaponConfigScriptableObject weaponConfig;

        public GameObject modelPrefab;

        public Vector3 spawnPoint;

        public Vector3 spawnRotation;
        
        protected GameObject Model;


        protected float AttackAnimationLength;

        public string attackAnimationName;
        
        protected Animator WeaponAnimator;

        protected MonoBehaviour ActiveMonoBehavior;

        public Transform weaponParent;
        
        public WeaponType weaponType;

        public delegate void PickedUpEvent(WeaponScriptableObject weapon);

        public static PickedUpEvent PickedUp;

        public bool Equipped
        {
            get => _equipped;
            set
            {
                _equipped = value;
                Model.SetActive(value);
            }
        }

        private bool _equipped;
        

        public virtual void Spawn(Transform parent, MonoBehaviour monoBehavior)
        {

            ActiveMonoBehavior = monoBehavior;

            weaponParent = parent;
            Model = Instantiate(modelPrefab, weaponParent, false);
            
            weaponParent.SetLocalPositionAndRotation(spawnPoint, Quaternion.Euler(spawnRotation));


            foreach (Transform child in Model.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

            Equipped = true;
        }
        
        public virtual void Attack(RaycastHit hit)
        {
            ActiveMonoBehavior.StartCoroutine(AttackCoroutine());

            TryDamage(hit.collider);


        }

        protected virtual void TryDamage(Collider col)
        {
            if (col && col.transform.root.TryGetComponent(out HealthHandler handler))
            {
                handler.healthScriptableObject.TakeDamage(weaponConfig.damage);
            }
        }
        
        private IEnumerator AttackCoroutine()
        {
            PlayAttackAnimation();
           
            yield return new WaitForSeconds(AttackAnimationLength / 2);
           
        }

    

        public void Destroy()
        {
            Destroy(Model);
        }

        public void SetWeaponLocation(Vector3 position)
        {
            weaponParent.transform.SetLocalPositionAndRotation(position, weaponParent.rotation);
        }

        public void ResetWeaponLocation()
        {
            Model.transform.SetLocalPositionAndRotation(new Vector3(0,0,0), Quaternion.Euler(0,0,0));
            weaponParent.transform.SetLocalPositionAndRotation(spawnPoint, Quaternion.Euler(spawnRotation));
        }

        protected virtual void PlayAttackAnimation()
        {
            WeaponAnimator.SetTrigger(attackAnimationName);

        }


    }
}
