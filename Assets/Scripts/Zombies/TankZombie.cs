using UnityEngine;

public class TankZombie : ZombieBase
{
    [Header("Tank Specific")]
    [SerializeField] private float damageReduction = 0.5f;
    [SerializeField] private float heavyAttackCooldown = 2.5f;

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



    protected override void Die()
    {
        // Tank falls slower — stays on ground longer before destroy
        Destroy(gameObject, 8f); // overrides the 5f in base
        base.Die();
    }

    //----------------------------------temporary-------------------------------------------------
    protected override void Update()
    {
        base.Update();

        // TEMP DEBUG — press K to instantly kill exploder, remove before final build
        if (Input.GetKeyDown(KeyCode.K))
            Die();
    }

    //----------------------------------temporary-------------------------------------------------
}