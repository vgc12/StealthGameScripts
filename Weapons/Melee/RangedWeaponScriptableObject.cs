using UnityEngine;

namespace Weapons.Melee
{
    [CreateAssetMenu(fileName = "Ranged Weapon Scriptable Object", menuName = "Weapons/Ranged Weapon", order = 0)]
    public class RangedWeaponScriptableObject : WeaponScriptableObject
    {
        public override void PlayAttackAnimation()
        {
            WeaponAnimator.SetTrigger(attackAnimationName);
        }
        
        public void Attack(Vector3 direction)
        {
            
        }
       
    }
}
