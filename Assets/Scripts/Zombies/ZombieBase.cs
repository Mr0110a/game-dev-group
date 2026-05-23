using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public abstract class ZombieBase : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] protected float detectionRange = 12f;
    [SerializeField] protected float fieldOfView = 110f;
    [SerializeField] protected float attackRange = 2.2f;

    [Header("Chase Behaviour")]
    [Tooltip("How far the zombie will chase you after losing sight.")]
    [SerializeField] protected float chaseRange = 20f;
    [Tooltip("How many seconds the zombie keeps chasing after losing line of sight.")]
    [SerializeField] protected float chaseMemoryDuration = 3f;
    protected float chaseMemoryTimer;

    [Header("Combat")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackCooldown = 1.5f;
    protected float lastAttackTime;
    protected float currentHealth;

    [Header("Patrol")]
    [SerializeField] protected Transform[] patrolPoints;
    [SerializeField] protected float waitTimeAtPoint = 2f;
    [SerializeField] protected float waypointReachedDist = 0.5f;
    protected int currentPatrolIndex;
    protected float waitTimer;
    protected bool isWaiting;

    [Header("Movement Speeds")]
    [SerializeField] protected float patrolSpeed = 3f;
    [SerializeField] protected float chaseSpeed = 4.5f;
    [SerializeField] protected float attackSpeedMultiplier = 1f;

    [Header("Animation Tuning")]
    [SerializeField] protected float walkAnimBaseSpeed = 3f;
    [SerializeField] protected float runAnimBaseSpeed = 3f;

    [Header("Animation Timing")]
    [SerializeField] protected float detectDuration = 1.2f;

    [Header("Audio Clips")]
    [SerializeField] protected AudioClip idleGroan;
    [SerializeField] protected AudioClip detectClip;
    [SerializeField] protected AudioClip footstepClip;
    [SerializeField] protected AudioClip attackSwingClip; // grunt/swing sound at start of attack motion
    [SerializeField] protected AudioClip attackClip;      // impact sound at the hit frame
    [SerializeField] protected AudioClip hurtClip;
    [SerializeField] protected AudioClip deathClip;
    [SerializeField][Range(0f, 1f)] protected float footstepVolume = 0.3f;

    [Header("Death Settings")]
    [SerializeField] private float deathSoundDelayBackward = 0f;
    [SerializeField] private float deathSoundDelayForward = 0f;

    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;
    protected Transform player;

    protected enum State { Patrolling, Detecting, Chasing, Attacking, Dead }
    protected State currentState = State.Patrolling;

    private bool attackBoolPendingReset = false;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        currentHealth = maxHealth;

        audioSource.spatialBlend = 1f;
        audioSource.minDistance = 3f;
        audioSource.maxDistance = 15f;
    }

    protected virtual void Start()
    {
        agent.speed = patrolSpeed;
        agent.acceleration = patrolSpeed * 2f;
        agent.angularSpeed = 360f;

        if (patrolPoints != null && patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[0].position);

        StartCoroutine(GroanRoutine());
    }

    protected virtual void Update()
    {
        if (currentState == State.Dead || player == null) return;

        switch (currentState)
        {
            case State.Patrolling:
                UpdatePatrol();
                if (CanSeePlayer())
                    StartDetect();
                break;

            case State.Detecting:
                // Coroutine handles transition to Chasing
                break;

            case State.Chasing:
                UpdateChase();

                float distToPlayer = Vector3.Distance(transform.position, player.position);

                if (CanSeePlayer())
                    chaseMemoryTimer = chaseMemoryDuration;
                else
                    chaseMemoryTimer -= Time.deltaTime;

                if (distToPlayer > chaseRange || chaseMemoryTimer <= 0f)
                    TransitionTo(State.Patrolling);
                else if (IsInAttackRange())
                    TransitionTo(State.Attacking);
                break;

            case State.Attacking:
                if (attackBoolPendingReset)
                {
                    animator.SetBool("Attack", false);
                    attackBoolPendingReset = false;
                }

                UpdateAttack();

                if (!IsInAttackRange())
                    TransitionTo(CanSeePlayer() ? State.Chasing : State.Patrolling);
                break;
        }

        UpdateAnimator();
    }

    protected virtual void StartDetect()
    {
        currentState = State.Detecting;
        agent.isStopped = true;
        animator.ResetTrigger("Detect");
        animator.SetTrigger("Detect");
        animator.SetFloat("Speed", 0f);
        PlaySound(detectClip, 1f);
        StartCoroutine(DetectRoutine());
    }

    protected virtual IEnumerator DetectRoutine()
    {
        float timer = 0f;

        while (timer < detectDuration)
        {
            if (currentState == State.Dead) yield break;
            timer += Time.deltaTime;
            yield return null;
        }

        if (currentState == State.Detecting)
            TransitionTo(State.Chasing);
    }

    protected virtual bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        Vector3 eyePos = transform.position + Vector3.up * 1.0f;
        int layerMask = ~LayerMask.GetMask("Zombie");

        if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, detectionRange, layerMask))
        {
            return hit.transform.CompareTag("Player");
        }

        return true;
    }

    protected virtual bool IsInAttackRange()
    {
        return Vector3.Distance(transform.position, player.position) <= attackRange;
    }

    protected virtual void UpdatePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            animator.SetFloat("Speed", 0f);
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                SetNextPatrolPoint();
            }
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= waypointReachedDist)
        {
            isWaiting = true;
            waitTimer = 0f;
            agent.isStopped = true;
        }
    }

    protected virtual void UpdateChase()
    {
        // Don't move until Animator has finished Detect transition
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInRunState = stateInfo.IsName("Zombie_Run") || stateInfo.IsName("Zombie_Walk");

        if (!isInRunState && currentState == State.Chasing)
        {
            // Still transitioning from Detect — stay stopped but face player
            agent.isStopped = true;
            Vector3 lookPos = player.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookPos), Time.deltaTime * 10f);
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.SetDestination(player.position);
    }

    protected virtual void UpdateAttack()
    {
        Vector3 lookPos = player.position - transform.position;
        lookPos.y = 0;
        if (lookPos != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        agent.isStopped = true;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            animator.SetBool("Attack", true);
            attackBoolPendingReset = true;
            // Sound is fired by Animation Event (PlayAttackSound) at the hit frame — not here
        }
    }

    // Called by Animation Event at the START of the attack motion (grunt/swing sound)
    public virtual void PlayAttackSwingSound()
    {
        if (currentState == State.Attacking)
            PlaySound(attackSwingClip, 0.8f);
    }

    // Called by Animation Event at the exact hit frame (impact sound)
    public virtual void PlayAttackSound()
    {
        if (currentState == State.Attacking)
            PlaySound(attackClip, 0.8f);
    }

    protected virtual void TransitionTo(State newState)
    {
        if (currentState == newState) return;

        if (currentState == State.Attacking)
        {
            animator.SetBool("Attack", false);
            attackBoolPendingReset = false;
        }

        currentState = newState;

        switch (newState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                if (!isWaiting && patrolPoints != null && patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                break;

            case State.Chasing:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                chaseMemoryTimer = chaseMemoryDuration;
                break;

            case State.Attacking:
                agent.isStopped = true;
                // CRITICAL: Set Attack bool immediately so Animator transitions before IsChasing drops
                animator.SetBool("Attack", true);
                attackBoolPendingReset = true;
                lastAttackTime = Time.time;
                // Sound is fired by Animation Event (PlayAttackSound) at the hit frame — not here
                break;
        }
    }

    protected virtual void SetNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
    }

    protected virtual void UpdateAnimator()
    {
        float speedPercent = 0f;
        float animPlaySpeed = 1f;

        if (agent.enabled && currentState != State.Dead && currentState != State.Detecting)
        {
            if (currentState == State.Patrolling)
            {
                if (isWaiting || patrolPoints == null || patrolPoints.Length == 0)
                    speedPercent = 0f;
                else
                    speedPercent = 0.5f;

                if (!isWaiting && patrolPoints != null && patrolPoints.Length > 0)
                    animPlaySpeed = patrolSpeed / walkAnimBaseSpeed;
            }
            else if (currentState == State.Chasing || currentState == State.Attacking)
            {
                // IsChasing stays true during Attacking so Animator doesn't drop to Idle
                speedPercent = Mathf.Clamp01(agent.velocity.magnitude / chaseSpeed);

                float currentSpeed = agent.velocity.magnitude;
                if (currentSpeed > 0.1f)
                    animPlaySpeed = currentSpeed / runAnimBaseSpeed;
            }
        }

        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsChasing", currentState == State.Chasing || currentState == State.Attacking);
        animator.SetBool("InAttackRange", IsInAttackRange());
        animator.SetFloat("AnimSpeed", animPlaySpeed);
    }

    public virtual void PlayFootstep()
    {
        if (footstepClip != null && agent.velocity.magnitude > 0.1f)
            audioSource.PlayOneShot(footstepClip, footstepVolume);
    }

    protected virtual void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    protected virtual IEnumerator GroanRoutine()
    {
        while (currentState != State.Dead)
        {
            float wait = Random.Range(5f, 15f);
            yield return new WaitForSeconds(wait);

            if (currentState != State.Dead && idleGroan != null && Random.value > 0.5f)
                PlaySound(idleGroan, 0.4f);
        }
    }

    // Called by damage system — passes source position for directional death
    public virtual void TakeDamage(float damage, Vector3 damageSourcePosition)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        PlaySound(hurtClip, 0.7f);

        if (currentHealth <= 0)
            Die(damageSourcePosition);
    }

    // Overload for compatibility — uses player position as source
    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, player != null ? player.position : transform.position);
    }

    protected virtual void Die(Vector3 damageSourcePosition)
    {
        currentState = State.Dead;
        agent.enabled = false;

        // Determine fall direction based on where damage came from
        Vector3 directionToSource = (damageSourcePosition - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, directionToSource);

        // dot > 0 = source is in front = zombie falls forward
        // dot < 0 = source is behind = zombie falls backward
        bool dieForward = dot > 0f;

        animator.SetBool("DieForward", dieForward);
        animator.SetTrigger("Death");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        float delay = dieForward ? deathSoundDelayForward : deathSoundDelayBackward;
        StartCoroutine(PlayDeathSound(delay));

        // Start dissolve — ZombieDissolve handles body cleanup
        GetComponent<ZombieDissolve>()?.StartDissolve();
    }

    // Keep old Die() for anything that calls it directly
    protected virtual void Die()
    {
        Die(player != null ? player.position : transform.position);
    }

    private IEnumerator PlayDeathSound(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        PlaySound(deathClip, 1f);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var pt in patrolPoints)
                if (pt != null) Gizmos.DrawSphere(pt.position, 0.3f);
        }
    }
}