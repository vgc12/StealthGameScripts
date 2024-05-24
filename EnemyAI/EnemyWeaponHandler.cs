using System.Collections;
using RenownedGames.AITree;
using UnityEngine;
using UnityEngine.Pool;
using PlayerScripts;

namespace EnemyAI
{
    public class EnemyWeaponHandler : MonoBehaviour
    {
        public GameObject ThrowingKnifePrefab;

        [SerializeField] private float spawnRate = .3f;


        public static ObjectPool<GameObject> ThrowingKnifePool;

        [SerializeField] private Transform throwingPosition;
        
        private Blackboard blackboard;
        private bool isAttacking;
        private void Awake()
        {
            ThrowingKnifePool = new ObjectPool<GameObject>(CreateThrowingKnife, GetThrowingKnife, ReleaseThrowingKnife, Destroy,true, 10, 50);
            isAttacking = false;
            blackboard = GetComponent<BehaviourRunner>().GetBlackboard();
        }


        private GameObject CreateThrowingKnife()
        {
            GameObject throwingKnife = Instantiate(ThrowingKnifePrefab);
       
            return throwingKnife;
        }
        private void GetThrowingKnife(GameObject throwingKnife)
        {
            throwingKnife.SetActive(true);
        }

        private void ReleaseThrowingKnife(GameObject throwingKnife)
        {
            throwingKnife.SetActive(false);
        }

        public void AttackFromRange(int amountToThrow)
        {
            if (isAttacking)
                return;
            StartCoroutine(ThrowKnives(amountToThrow));
            isAttacking = true;
        }
    
        private IEnumerator ThrowKnives(int amountToThrow)
        {
            
            
            yield return new WaitForSeconds(1);
            for (var i = amountToThrow - 1; i >= 0; i--)
            {
                var throwingKnifeObject = ThrowingKnifePool.Get();
                ThrowingKnife throwingKnife = throwingKnifeObject.GetComponent<ThrowingKnife>();
                throwingKnife.target = PlayerScripts.Player.Instance.HealthHandler.targets[Random.Range(0, Player.Instance.HealthHandler.targets.Length)];
                throwingKnife.transform.position = throwingPosition.position;
                throwingKnife.readyToThrow = true;
                AITreeHelper.SetBlackboardValue(blackboard, "KnivesLeft", i);
                yield return new WaitForSeconds(spawnRate);
            }

          
        
        }
    
   
    }
}
