using System.Threading;
using System.Threading.Tasks;
using Damageables;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace EnemyAI
{
    public enum EnemyAnimationState
    {
        Idle,
        Walking,
        Running,
        Attack
    }

    public abstract class EnemyBase : MonoBehaviour
    {

        public static UnityEvent<EnemyBase> PlayerDetectedEvent;

   
        [FormerlySerializedAs("Player")] public GameObject player;

        [FormerlySerializedAs("EnemyHead")] public Transform enemyHead;

        [FormerlySerializedAs("NotifyRange")] [Tooltip("How far away an enemy can shout to other enemies")]
        public float notifyRange;

        [FormerlySerializedAs("FovAngle")] [Range(0, 360)]
        public float fovAngle;

        [FormerlySerializedAs("DetectionRange")] public float detectionRange;

        [FormerlySerializedAs("CanSeePlayer")] public bool canSeePlayer;
        [FormerlySerializedAs("PlayerDetected")] public bool playerDetected;

        [FormerlySerializedAs("DetectionMeter")]
        [Space]
        [Header("Detection Meter")]
        public float detectionMeter;

        [FormerlySerializedAs("DetectionMult")] public float detectionMult;

        private float _distanceToPlayer;

        [FormerlySerializedAs("PatrolPoints")] [SerializeField]
        private Transform[] patrolPoints;

        private NavMeshAgent _agent;

        private Vector3 _lastKnownPlayerPosition;

        [FormerlySerializedAs("_walkAnimationHash")] [SerializeField]
        private int walkAnimationHash;

        [FormerlySerializedAs("_runAnimationHash")] [SerializeField]
        private int runAnimationHash;

        [FormerlySerializedAs("_attackAnimationHash")] [SerializeField]
        private int attackAnimationHash;

        [FormerlySerializedAs("Animator")] public Animator animator;

        private float _attackAnimationLength;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CancellationTokenSource _cancellationWaitForSeconds = new();

        //Head Rigging
        private Rig _headRig;

        [FormerlySerializedAs("_detectionMask")] [SerializeField]
        private LayerMask detectionMask;

        [FormerlySerializedAs("_obstructionMask")] [SerializeField]
        private LayerMask obstructionMask;


        private async void Start()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");

            }
            _agent = GetComponent<NavMeshAgent>();

            PlayerDetectedEvent ??= new UnityEvent<EnemyBase>();

            PlayerDetectedEvent?.AddListener(OtherEnemeyDetectedPlayer);

            walkAnimationHash = Animator.StringToHash("isWalking");
            runAnimationHash = Animator.StringToHash("isRunning");
            _headRig = GetComponentInChildren<Rig>();
            _headRig.weight = 0;
            //_attackAnimationHash = Animator.StringToHash("isAttacking");

            //_attackAnimationLength = Animator.runtimeAnimatorController.animationClips.First(anim => anim.name == "Attack").length;
       
     
            await Task.WhenAll(FOV(_cancellationTokenSource.Token), Patrol(_cancellationTokenSource.Token));
        }

        private void OtherEnemeyDetectedPlayer(EnemyBase enemyBase)
        {
            if (enemyBase != this && playerDetected != true)
            {
                if (Vector3.Distance(transform.position, enemyBase.transform.position) < notifyRange)
                {
                    _lastKnownPlayerPosition = enemyBase._lastKnownPlayerPosition;
                    playerDetected = true;
                }
            }
        }

 


        private void Update()
        {
            _headRig.weight = detectionMeter;

            if (canSeePlayer)
            {
                RaiseDetection();
            }
            else
            {
                LowerDetection();
            }

            //Show player that enemy is aware something is up
      

            //Enemy sees player fully and is chasing
            if (playerDetected)
            {
                bool isRunning = animator.GetBool(runAnimationHash);
                _cancellationWaitForSeconds.Cancel();
                transform.LookAt(_lastKnownPlayerPosition);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                if ((_lastKnownPlayerPosition - transform.position).magnitude > 2)
                {
                    _agent.speed = 3f;
                    _agent.destination = _lastKnownPlayerPosition;
                    if(!isRunning)
                        animator.SetBool(runAnimationHash, true);
                }
                else
                {
              
                    _agent.destination = transform.position;
                    Physics.Raycast(transform.position, transform.forward, out var hit, 2f, detectionMask);
                    //await Attack(hit);
                
                }
            }



            ResetDetectionMeter();
        }

        protected virtual async Task Attack(RaycastHit hit)
        {
            //Animator.SetTrigger("Attack");
            await Awaitable.WaitForSecondsAsync(2);
            if (hit.collider != null && hit.transform.root.TryGetComponent(out HealthHandler handler))
            {
                handler.healthScriptableObject.TakeDamage(100);
            }

            await Awaitable.NextFrameAsync();
        }

        private async Task Patrol(CancellationToken token)
        {
            _agent.updateRotation = false;
            Transform destination = patrolPoints[UnityEngine.Random.Range(0, patrolPoints.Length)];
            transform.rotation = Quaternion.LookRotation(destination.position - transform.position);
            while (true && !token.IsCancellationRequested)
            {
                if ( detectionMeter <.6f && patrolPoints != null)
                {
                    bool isWalking = animator.GetBool(walkAnimationHash);
                    if (!isWalking)
                        animator.SetBool(walkAnimationHash, true);

                    Debug.Log(_agent.destination == _lastKnownPlayerPosition);

                    _agent.destination = destination.position;
                    if ((destination.position - transform.position).magnitude <= 2f)
                    {

                        animator.SetBool(walkAnimationHash, false);
                        _agent.destination = transform.position;
                        destination = patrolPoints[UnityEngine.Random.Range(0, patrolPoints.Length)];
                        await Awaitable.WaitForSecondsAsync(UnityEngine.Random.Range(1, 3), _cancellationWaitForSeconds.Token);
                        await TurnTowardsTargetAsync(destination.position, token);

                    }
                }
                else if (detectionMeter > .6f&& !playerDetected)
                {
                    _agent.destination = transform.position;
                    await TurnTowardsTargetAsync(_lastKnownPlayerPosition, token);
                    _agent.destination = _lastKnownPlayerPosition;
                }

                await Awaitable.NextFrameAsync(token);

            }
        }

        private async Task TurnTowardsTargetAsync(Vector3 destination, CancellationToken token)
        {

            Quaternion lookRotation = Quaternion.LookRotation(destination - transform.position);
            float time = 0;
            while (time < 1 && !_cancellationWaitForSeconds.Token.IsCancellationRequested)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);
                transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
                time += Time.deltaTime * .25f;
                await Awaitable.NextFrameAsync(token);
            }
            await Awaitable.NextFrameAsync(token);
        }
  




        public virtual void LowerDetection()
        {
            detectionMeter -= Time.deltaTime * .12f;
        }

        private void RaiseDetection()
        {
            detectionMeter += Time.deltaTime * .12f / (1 / (_distanceToPlayer * detectionMult));
            if (detectionMeter > .6f)
            {
                _lastKnownPlayerPosition = player.transform.position;

            }

            if (detectionMeter > .99f)
            {
                if (!playerDetected)
                {
                    PlayerDetectedEvent?.Invoke(this);
                }
                playerDetected = true;
            }
        }




        private void ResetDetectionMeter()
        {
            if (detectionMeter < 0)
            {
                detectionMeter = 0;
            }
            else if (detectionMeter > 1)
            {
                detectionMeter = 1;
            }
        }

        private async Task FOV(CancellationToken token)
        {
            float delay = .2f;

            while (true && !token.IsCancellationRequested)
            {
                await Awaitable.WaitForSecondsAsync(delay, token);
                await Awaitable.NextFrameAsync(token);
                FieldOfViewCheck();
            }

        }
        private void FieldOfViewCheck()
        {
            Collider[] rangeChecks = Physics.OverlapSphere(enemyHead.position, detectionRange, detectionMask);

            if (rangeChecks.Length != 0)
            {
                Transform target = rangeChecks[0].transform;
                Vector3 directionToTarget = (target.position - enemyHead.position).normalized;

                if (Vector3.Angle(enemyHead.forward, directionToTarget) < fovAngle / 2)
                {
                    _distanceToPlayer = Vector3.Distance(enemyHead.position, target.position);

                    if (Physics.Raycast(enemyHead.position, directionToTarget, out var hit, _distanceToPlayer, obstructionMask))
                    {
                        canSeePlayer = false;
                    }
                    else
                    {
                        _lastKnownPlayerPosition = hit.point;
                        canSeePlayer = true;
                    }
                }
                else
                {
                    canSeePlayer = false;
                }
            }
            else
            {
                canSeePlayer = false;
            }
        }

        private void OnDestroy()
        {
            _cancellationTokenSource.Cancel();
            _agent.enabled = false;
        
        }
    }
}