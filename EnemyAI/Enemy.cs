using Damageables;
using RenownedGames.AITree;
using UnityEngine;
using UnityEngine.AI;
using PlayerScripts;

namespace EnemyAI
{
    public enum EnemyState
    {
        
        InCombat,
        Suspicious,
        Patrolling,
        Dead
    }
    [RequireComponent(typeof(PlayerSensor),typeof(BehaviourRunner), typeof(NavMeshAgent))]
    
    public class Enemy : MonoBehaviour
    {
    
    
        [SerializeField] private HealthHandler playerHealthHandler;

        [SerializeField] private BehaviourRunner behaviorRunner;

        [SerializeField] private AttackConfigScriptableObject attackConfig;
        [SerializeField] private PlayerSensor playerSensor;
        //[SerializeField] private AttackSensor attackSensor;
        [SerializeField] private float lastAttackTime;
        [SerializeField] private bool inAttackRange;
        [SerializeField] private Transform[] targets;
        private int numberOfTargets;
        private int currentTargetIndex;
        private Blackboard enemyBlackboard;
        private NavMeshAgent agent;
        public Transform inCombatFocusPoint;
        private HealthHandler healthHandler;
        

        
        private void Start()
        {
            playerSensor = GetComponent<PlayerSensor>();
            //attackSensor = GetComponent<AttackSensor>();
            healthHandler = GetComponent<HealthHandler>();
            behaviorRunner = GetComponent<BehaviourRunner>();
            agent = GetComponent<NavMeshAgent>();
            enemyBlackboard = behaviorRunner.GetBlackboard();
            lastAttackTime = Time.time;
            playerSensor.PlayerDetectedEvent += OnPlayerDetected;
            playerSensor.SuspicionRaisedEvent += OnSuspicionRaised;
            playerSensor.PlayerLostEvent += OnPlayerLost;
            healthHandler.healthScriptableObject.Death += OnDeath;
            
            //attackSensor.PlayerEnterEvent += AttackSensorOnPlayerEnter;
            //attackSensor.PlayerExitEvent += AttackSensorOnPlayerExit;
            foreach (var target in targets)
            {
                target.SetParent(null, true);
            }
            numberOfTargets = targets.Length;
            currentTargetIndex = 0;
            AITreeHelper.SetBlackboardValue(enemyBlackboard, "Target", targets[currentTargetIndex].position);
        }
        
        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            playerSensor.PlayerDetectedEvent -= OnPlayerDetected;
            playerSensor.SuspicionRaisedEvent -= OnSuspicionRaised;
            playerSensor.PlayerLostEvent -= OnPlayerLost;
            healthHandler.healthScriptableObject.Death -= OnDeath;
    
        }

        private void OnSuspicionRaised(Vector3 lastKnownPosition)
        {
            AITreeHelper.SetBlackboardValue(enemyBlackboard, "Target", lastKnownPosition);
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Detected));
        }

        private void OnPlayerLost(Vector3 lastKnownPosition)
        {
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Undetected));
        }

        public void OnPlayerDetected(Transform player)
        {
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Detected, inCombatFocusPoint.transform));
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime*.9f); 
           
        }

 
        


        public void ChooseTarget()
        {
            currentTargetIndex++;
            currentTargetIndex %= numberOfTargets;
            AITreeHelper.SetBlackboardValue(enemyBlackboard, "Target", targets[currentTargetIndex].position);
        }
        
    
   
        

        private void OnDeath()
        {
            PlayerStateInfo playerStateInfo = new PlayerStateInfo(PlayerState.Undetected);
            this.enabled = false;
            behaviorRunner.enabled = false;
            gameObject.SetActive(false);
            playerSensor.enabled = false;
        }

    }
}
