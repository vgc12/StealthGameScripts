using System.Threading;
using System.Threading.Tasks;
using RenownedGames.AITree;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace EnemyAI
{
    [RequireComponent(typeof(BehaviourRunner))]
    public class PlayerSensor : MonoBehaviour
    {
        public delegate void OnPlayerDetected(Transform player);
        public delegate void OnSuspicionRaised(Vector3 lastKnownPosition);
        public delegate void OnSuspicionLost(Vector3 lastKnownPosition);
        public delegate void OnPlayerLost(Vector3 lastKnownPosition);
        
        public event OnPlayerDetected PlayerDetectedEvent;
        public event OnSuspicionRaised SuspicionRaisedEvent;
        public event OnPlayerLost PlayerLostEvent;


        public GameObject player;

        public Transform enemyHead;

        [Tooltip("How far away an enemy can shout to other enemies")]
        public float notifyRange;

        [Range(0, 360)]
        public float fovAngle;

        public float detectionRange;

        public bool canSeePlayer;
        public bool playerDetected;

        public MultiAimConstraint constraint;

        [Header("Detection Meter")]
        public float detectionMeter;

        private float distanceToPlayer;

        private Vector3 lastKnownPlayerPosition;

        private BehaviourRunner behaviorRunner;

        private Blackboard enemyBlackboard;

        private readonly Collider[] results = new Collider[1]; 

        private bool allowTurnHead;

        private readonly CancellationTokenSource cancellationTokenSource = new();

    
        [SerializeField] private LayerMask detectionMask;

   
        [SerializeField] private LayerMask obstructionMask;

        private async void Start()
        {
            behaviorRunner = GetComponent<BehaviourRunner>();
            enemyBlackboard = behaviorRunner.GetBlackboard();
            constraint.weight = 0;
            await FOV(cancellationTokenSource.Token);
        }

        private void Update()
        {
            enemyBlackboard.TryGetKey("Player", out TransformKey target);

            if (canSeePlayer)
            {

                detectionMeter += Mathf.Pow(0.6f, Time.deltaTime * distanceToPlayer * 200f);
              
            }
            else
            {
                if (!playerDetected)
                {
                    detectionMeter -= Time.deltaTime * .12f;
                }
            }
           

            switch (detectionMeter)
            {
                
                case >= .99f:
                    playerDetected = true;
                    PlayerDetectedEvent?.Invoke(player.transform);
                    AITreeHelper.SetBlackboardValue(enemyBlackboard, "PlayerDetected", true);
                    break;
                case >= .6f:
                    SuspicionRaisedEvent?.Invoke(lastKnownPlayerPosition);
                    allowTurnHead = true;
                    break;
           
                    
            }
                

            TurnEnemyHead();

            detectionMeter = Mathf.Clamp(detectionMeter, 0, 1);
           
            AITreeHelper.SetBlackboardValue(enemyBlackboard, "DetectionMeter", detectionMeter);
        }

        private void TurnEnemyHead()
        {
            bool playerInHeadTurnRange = Vector3.Angle(transform.forward, (player.transform.position - enemyHead.position).normalized) < 90;

            if (allowTurnHead && playerInHeadTurnRange)
                constraint.weight += Time.deltaTime * .3f;
            else
                constraint.weight -= Time.deltaTime * .3f;
        }

        private async Task FOV(CancellationToken token)
        {
            float delay = .2f;
        

            while (!token.IsCancellationRequested)
            {
                await Awaitable.WaitForSecondsAsync(delay, token);
                await Awaitable.NextFrameAsync(token);
                FieldOfViewCheck();
            }
        }

        private void FieldOfViewCheck()
        {
            int size = Physics.OverlapSphereNonAlloc(enemyHead.position, detectionRange, results, detectionMask);

            if (size <= 0)
            {
                canSeePlayer = false;
                return;
            }

            
            Vector3 directionToTarget = (player.transform.position - enemyHead.position).normalized;

            if (Vector3.Angle(enemyHead.forward, directionToTarget) < fovAngle / 2)
            {
                distanceToPlayer = Vector3.Distance(enemyHead.position, player.transform.position);
         

                if (Physics.Raycast(enemyHead.position, directionToTarget, out var hit, distanceToPlayer, obstructionMask))
                {
                    canSeePlayer = false;
                }
                else
                {
                    lastKnownPlayerPosition = hit.point;
                    canSeePlayer = true;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        
        }
    }
}

