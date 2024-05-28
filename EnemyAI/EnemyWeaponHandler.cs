using System.Collections;
using PlayerScripts;
using RenownedGames.AITree;
using UnityEngine;
using UnityEngine.Pool;

namespace EnemyAI
{
    public class EnemyWeaponHandler : MonoBehaviour
    {
        public GameObject throwingKnifePrefab;

        [SerializeField] private float spawnRate = .3f;


        public static ObjectPool<GameObject> ThrowingKnifePool;

        [SerializeField] private Transform throwingPosition;
        
        private Blackboard _blackboard;
        private bool _isAttacking;
        private void Awake()
        {
            ThrowingKnifePool = new ObjectPool<GameObject>(CreateThrowingKnife, GetThrowingKnife, ReleaseThrowingKnife, Destroy,true, 10, 50);
            _isAttacking = false;
            _blackboard = GetComponent<BehaviourRunner>().GetBlackboard();
        }


        private GameObject CreateThrowingKnife()
        {
            GameObject throwingKnife = Instantiate(throwingKnifePrefab);
       
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
            if (_isAttacking)
                return;
            StartCoroutine(ThrowKnives(amountToThrow));
            _isAttacking = true;
        }
    
        private IEnumerator ThrowKnives(int amountToThrow)
        {
            
            
            yield return new WaitForSeconds(1);
            for (var i = amountToThrow - 1; i >= 0; i--)
            {
                
                if (Player.Instance.playerState == PlayerState.Dead)
                {
                    break;
                }
                var throwingKnifeObject = ThrowingKnifePool.Get();
                ThrowingKnife throwingKnife = throwingKnifeObject.GetComponent<ThrowingKnife>();
                
                throwingKnife.target = Player.Instance.HealthHandler.targets[Random.Range(0, Player.Instance.HealthHandler.targets.Length)];
                throwingKnife.transform.position = throwingPosition.position;
                throwingKnife.readyToThrow = true;
                yield return new WaitForSeconds(spawnRate);
                AITreeHelper.SetBlackboardValue(_blackboard, "KnivesLeft", i);
                throwingKnife.lastKnife = i == 0;
            }

            
        
        }
    
   
    }
}
