using UnityEngine;

public class RunnerZombie : ZombieBase
{
    // Runner specific values are set directly in the Inspector via ZombieBase fields
    // No extra parameters needed — just tune Detection Range, FOV, Chase Speed, and Detect Duration there

    protected override void UpdateChase()
    {
        // Override needed because Runner's run state is named "Runner_run" not "Zombie_Run"
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        bool isInRunState = stateInfo.IsName("Runner_run") || stateInfo.IsName("Zombie_Walk");

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

    protected override void UpdateAnimator()
    {
        // Runner override: freeze movement during attack but keep chasing state active
        if (currentState == State.Attacking)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", true);
            animator.SetBool("InAttackRange", IsInAttackRange());
            animator.SetFloat("AnimSpeed", 1f);
            return;
        }

        // Fall back to base for everything else
        base.UpdateAnimator();
    }

    // TEMP DEBUG — press K to instantly kill runner, remove before final build
    protected override void Update()
    {
        base.Update();
        if (Input.GetKeyDown(KeyCode.K))
            Die();
    }
}