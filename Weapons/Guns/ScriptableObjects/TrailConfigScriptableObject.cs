using UnityEngine;

namespace Weapons.Guns.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Trail Configuration", menuName = "Guns/Trail Configuration", order = 2)]
    public class TrailConfigScriptableObject : ScriptableObject
    {
        public Material TrailMaterial;
        public AnimationCurve WidthCurve;
        public float Duration = 0.5f;
        public float MinVertexDistance = 0.1f;
        public Gradient Color;

        [Tooltip("Max Distance before counted as miss")]
        public float MissDistance = 100f;

        [Tooltip("How fast the trail will move towards the target position in U/S")]
        public float TrailSpeed = 1f;
    }
}
