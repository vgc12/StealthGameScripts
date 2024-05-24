using RenownedGames.AITree;
using UnityEngine;

namespace EnemyAI
{
    public static class AITreeHelper 
    {
        public static void SetBlackboardValue(Blackboard blackboard, string key, Transform value)
        {
            if (blackboard.TryGetKey(key, out TransformKey transformKey))
            {
                transformKey.SetValue(value);
            }
        }

        public static void SetBlackboardValue(Blackboard blackboard, string key, bool value)
        {
            if (blackboard.TryGetKey(key, out BoolKey transformKey))
            {
                transformKey.SetValue(value);
            }
        }

        public static void SetBlackboardValue(Blackboard blackboard, string key, float value)
        {
            if (blackboard.TryGetKey(key, out FloatKey transformKey))
            {
                transformKey.SetValue(value);
            }
        }

        public static void SetBlackboardValue(Blackboard blackboard, string key, int value)
        {
            if (blackboard.TryGetKey(key, out IntKey transformKey))
            {
                transformKey.SetValue(value);
            }
        }

        public static void SetBlackboardValue(Blackboard blackboard, string key, string value)
        {
            if (blackboard.TryGetKey(key, out StringKey transformKey))
            {
                transformKey.SetValue(value);
            }
        }

        public static void SetBlackboardValue(Blackboard blackboard, string key, Vector3 value)
        {
            if (blackboard.TryGetKey(key, out Vector3Key transformKey))
            {
                transformKey.SetValue(value);
            }
        }
    }
}
