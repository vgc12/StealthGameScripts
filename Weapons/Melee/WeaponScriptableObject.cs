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

        public Vector3 hitPosition;

        protected GameObject model;

        public UnityEvent swingEvent;

        protected float attackAnimationLength;

        public string attackAnimationName;

     

        protected Animator WeaponAnimator;

        protected MonoBehaviour activeMonoBehavior;

        public Transform weaponParent;
        
        public WeaponType weaponType;
        
        public bool Equipped { get; set; }
        

        public virtual void Spawn(Transform parent, MonoBehaviour monoBehavior)
        {

            this.activeMonoBehavior = monoBehavior;

            weaponParent = parent;
            model = Instantiate(modelPrefab, weaponParent, false);
            
            weaponParent.SetLocalPositionAndRotation(spawnPoint, Quaternion.Euler(spawnRotation));


            foreach (Transform child in model.transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Weapon");
            }

        }

        public void Enable()
        {
            model.SetActive(true);
            Equipped = true;
        }

        public void Disable()
        {
            model.SetActive(false);
            Equipped = false;
        }

        public void Destroy()
        {
            Destroy(model);
        }

        public void SetWeaponLocation(Vector3 position)
        {
            weaponParent.transform.SetLocalPositionAndRotation(position, weaponParent.rotation);
        }

        public void ResetWeaponLocation()
        {
            weaponParent.transform.SetLocalPositionAndRotation(spawnPoint, Quaternion.Euler(spawnRotation));
        }

        public virtual void PlayAttackAnimation()
        {
            WeaponAnimator.SetTrigger(attackAnimationName);

        }


    }
}
