using UnityEngine;

public class TankZombie : ZombieBase
{
    [Header("Tank Specific")]
    [SerializeField] private float damageReduction = 0.5f;
    [SerializeField] private float heavyAttackCooldown = 2.5f;
    [SerializeField] private AudioClip heavyAttackSwingClip; // grunt/swing override
    [SerializeField] private AudioClip heavyAttackClip;      // impact override

    protected override void Awake()
    {
        base.Awake();
        attackCooldown = heavyAttackCooldown;
    }

    public override void TakeDamage(float damage)
    {
        float reduced = damage * (1f - damageReduction);
        base.TakeDamage(reduced);
    }

    public override void PlayAttackSwingSound()
    {
        if (currentState == State.Attacking)
            PlaySound(heavyAttackSwingClip != null ? heavyAttackSwingClip : attackSwingClip, 1f);
    }

    public override void PlayAttackSound()
    {
        if (currentState == State.Attacking)
            PlaySound(heavyAttackClip != null ? heavyAttackClip : attackClip, 1f);
    }

    protected override void Die()
    {
        // Tank falls slower — stays on ground longer before destroy
        Destroy(gameObject, 8f); // overrides the 5f in base
        base.Die();
    }
}