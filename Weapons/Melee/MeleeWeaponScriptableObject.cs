using System.Collections;
using System.Linq;
using Damageables;
using UnityEngine;

namespace Weapons.Melee
{
    [CreateAssetMenu(fileName = "Melee Weapon Scriptable Object", menuName = "Weapons/Melee Weapon", order = 0)]
    public class MeleeWeaponScriptableObject : WeaponScriptableObject
    {


      

        public new void Spawn(Transform parent, MonoBehaviour monoBehavior)
        {

            base.Spawn(parent, monoBehavior);
            WeaponAnimator = model.GetComponent<Animator>();
            attackAnimationLength = WeaponAnimator.runtimeAnimatorController.animationClips
                .First(anim => anim.name == attackAnimationName).length;
      
        }

        public virtual void Attack(RaycastHit hit)
        {
            activeMonoBehavior.StartCoroutine(AttackCoroutine());

            if (hit.collider != null && hit.collider.transform.root.TryGetComponent(out HealthHandler handler))
            {
                handler.healthScriptableObject.TakeDamage(weaponConfig.Damage);
            }

            swingEvent?.Invoke();

        }


        private IEnumerator AttackCoroutine()
        {
            PlayAttackAnimation();
           
            yield return new WaitForSeconds(attackAnimationLength / 2);
           
        }

        
    }
}
