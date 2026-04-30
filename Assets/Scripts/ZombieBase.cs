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

    [Header("Audio Clips")]
    [SerializeField] protected AudioClip idleGroan;
    [SerializeField] protected AudioClip footstepClip;
    [SerializeField] protected AudioClip attackClip;
    [SerializeField] protected AudioClip hurtClip;
    [SerializeField] protected AudioClip deathClip;
    [SerializeField][Range(0f, 1f)] protected float footstepVolume = 0.3f;

    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;
    protected Transform player;

    protected enum State { Patrolling, Chasing, Attacking, Dead }
    protected State currentState = State.Patrolling;

    public abstract float PatrolSpeed { get; }
    public abstract float ChaseSpeed { get; }
    public abstract float AttackSpeedMultiplier { get; }

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
        agent.speed = PatrolSpeed;
        agent.acceleration = PatrolSpeed * 2f;
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
                    TransitionTo(State.Chasing);
                break;

            case State.Chasing:
                UpdateChase();
                if (!CanSeePlayer())
                    TransitionTo(State.Patrolling);
                else if (IsInAttackRange())
                    TransitionTo(State.Attacking);
                break;

            case State.Attacking:
                UpdateAttack();
                if (!IsInAttackRange())
                    TransitionTo(CanSeePlayer() ? State.Chasing : State.Patrolling);
                break;
        }

        UpdateAnimator();
    }

    protected virtual bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > fieldOfView * 0.5f) return false;

        // Line of sight check - hits anything except zombie itself
        Vector3 eyePos = transform.position + Vector3.up * 1.0f;

        // Ignore zombies layer so they don't block each other's vision
        int layerMask = ~LayerMask.GetMask("Zombie");

        if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, detectionRange, layerMask))
        {
            return hit.transform.CompareTag("Player");
        }

        // If raycast hits nothing, assume we can see (open space)
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
        agent.isStopped = false;
        agent.speed = ChaseSpeed;
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
            animator.SetTrigger("Attack");
            PlaySound(attackClip, 0.8f);

            // We'll connect player damage later
            // player.GetComponent<PlayerHealth>()?.TakeDamage(attackDamage);
        }
    }

    protected virtual void TransitionTo(State newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        switch (newState)
        {
            case State.Patrolling:
                agent.speed = PatrolSpeed;
                agent.isStopped = false;
                if (!isWaiting && patrolPoints != null && patrolPoints.Length > 0)
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                break;

            case State.Chasing:
                agent.speed = ChaseSpeed;
                agent.isStopped = false;
                PlaySound(idleGroan, 0.6f);
                break;

            case State.Attacking:
                agent.isStopped = true;
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

        if (agent.enabled && !isWaiting && currentState != State.Dead)
        {
            float currentSpeed = agent.velocity.magnitude;

            if (currentState == State.Chasing)
                speedPercent = Mathf.Clamp01(currentSpeed / ChaseSpeed);
            else if (currentState == State.Patrolling)
                speedPercent = Mathf.Clamp01(currentSpeed / PatrolSpeed) * 0.5f;
        }

        animator.SetFloat("Speed", speedPercent);
        animator.SetBool("IsChasing", currentState == State.Chasing);
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

    public virtual void TakeDamage(float damage)
    {
        if (currentState == State.Dead) return;

        currentHealth -= damage;
        PlaySound(hurtClip, 0.7f);

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        currentState = State.Dead;
        agent.enabled = false;
        animator.SetTrigger("Death");
        PlaySound(deathClip, 1f);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Destroy(gameObject, 5f);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var pt in patrolPoints)
                if (pt != null) Gizmos.DrawSphere(pt.position, 0.3f);
        }
    }
}
