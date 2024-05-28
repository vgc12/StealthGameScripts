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
            WeaponAnimator = Model.GetComponent<Animator>();
            AttackAnimationLength = WeaponAnimator.runtimeAnimatorController.animationClips
                .First(anim => anim.name == attackAnimationName).length;
      
        }

       


        private IEnumerator AttackCoroutine()
        {
            PlayAttackAnimation();
           
            yield return new WaitForSeconds(AttackAnimationLength / 2);
           
        }

        
    }
}
