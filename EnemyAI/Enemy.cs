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
        [SerializeField] private bool inAttackRange;
        [SerializeField] private Transform[] targets;
        private int _numberOfTargets;
        private int _currentTargetIndex;
        private Blackboard _enemyBlackboard;
        public Transform inCombatFocusPoint;
        private HealthHandler _healthHandler;
        

        
        private void Start()
        {
            playerSensor = GetComponent<PlayerSensor>();
            _healthHandler = GetComponent<HealthHandler>();
            behaviorRunner = GetComponent<BehaviourRunner>();
            _enemyBlackboard = behaviorRunner.GetBlackboard();
            playerSensor.PlayerDetectedEvent += OnPlayerDetected;
            playerSensor.SuspicionRaisedEvent += OnSuspicionRaised;
            playerSensor.PlayerLostEvent += OnPlayerLost;
            _healthHandler.healthScriptableObject.Death += OnDeath;
            
            foreach (var target in targets)
            {
                target.SetParent(null, true);
            }
            
            _numberOfTargets = targets.Length;
            _currentTargetIndex = 0;
            AITreeHelper.SetBlackboardValue(_enemyBlackboard, "Target", targets[_currentTargetIndex].position);
            
            Player.Instance.PlayerStateChangedEvent += OnPlayerStateChanged;
        }

        private void OnPlayerStateChanged(PlayerStateInfo ps)
        {
            AITreeHelper.SetBlackboardValue(_enemyBlackboard, "PlayerIsDead", ps.State == PlayerState.Dead);
            Debug.Log(ps.State);
        }

        private void Update()
        {
            
        }

        private void OnDestroy()
        {
            playerSensor.PlayerDetectedEvent -= OnPlayerDetected;
            playerSensor.SuspicionRaisedEvent -= OnSuspicionRaised;
            playerSensor.PlayerLostEvent -= OnPlayerLost;
            _healthHandler.healthScriptableObject.Death -= OnDeath;
    
        }

        private void OnSuspicionRaised(Vector3 lastKnownPosition)
        {
            AITreeHelper.SetBlackboardValue(_enemyBlackboard, "Target", lastKnownPosition);
       
        }

        private void OnPlayerLost(Vector3 lastKnownPosition)
        {
            if (Player.Instance.playerState != PlayerState.Undetected)
            {
                Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.Undetected));
            }
        }

        public void OnPlayerDetected(Transform player)
        {
            
            Player.Instance.ChangePlayerState(new PlayerStateInfo(PlayerState.InCombat,
                inCombatFocusPoint.transform));
            

            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            Quaternion rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime*.9f); 
           
        }

 
        


        public void ChooseTarget()
        {
            _currentTargetIndex++;
            _currentTargetIndex %= _numberOfTargets;
            AITreeHelper.SetBlackboardValue(_enemyBlackboard, "Target", targets[_currentTargetIndex].position);
        }
        
    
   
        

        private void OnDeath()
        {
            var playerStateInfo = new PlayerStateInfo(PlayerState.Undetected);
            Player.Instance.ChangePlayerState(playerStateInfo);
            enabled = false;
            behaviorRunner.enabled = false;
            gameObject.SetActive(false);
            playerSensor.enabled = false;
        }

    }
}
