using UnityEngine;

public class RunnerZombie : ZombieBase
{
    [Header("Runner Specific")]
    [SerializeField] private float runnerDetectDuration = 0.6f;
    [SerializeField] private float runnerFOV = 140f;

    protected override void Awake()
    {
        base.Awake();
        detectDuration = runnerDetectDuration; // reacts instantly
        fieldOfView = runnerFOV; // wider peripheral vision
    }

    protected override void UpdateAnimator()
    {
        // Runner override: force zero movement during attack so he freezes in place
        if (currentState == State.Attacking)
        {
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsChasing", true); // keeps Run state active, just paused
            animator.SetFloat("AnimSpeed", 1f);
            return;
        }

        // Fall back to base for everything else (patrol, chase, detect)
        base.UpdateAnimator();
    }
}
