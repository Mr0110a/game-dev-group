using UnityEngine;

public class SpitterZombie : ZombieBase
{
    [Header("Spitter Specific")]
    [SerializeField] private Transform mouthTransform;
    [SerializeField] private GameObject acidProjectilePrefab;
    [SerializeField] private float spitForce = 15f;
    [SerializeField] private float spitAttackRange = 4f;
    [SerializeField] private AudioClip spitSound;

    [Header("Spit Tuning")]
    [SerializeField] private float spawnForwardOffset = 0f;   // tweak if spawning inside chest
    [SerializeField] private float targetHeightOffset = 1.4f; // 1.4 = chest/neck level

    protected override void Awake()
    {
        base.Awake();
        attackRange = spitAttackRange;
    }

    // Called by Animation Event at the peak of the attack animation
    public void OnSpitAttack()
    {
        if (currentState != State.Attacking || player == null) return;

        if (acidProjectilePrefab != null && mouthTransform != null)
        {
            PlaySound(spitSound, 0.9f);

            // Use spawn pos from mouth, offset forward using zombie's OWN forward not bone forward
            Vector3 spawnPos = mouthTransform.position + transform.forward * spawnForwardOffset;

            // Aim at player's chest using world up offset from player's foot pivot
            Vector3 targetPos = player.position + Vector3.up * targetHeightOffset;

            // Direction purely from spawn to target, no bone axis dependency
            Vector3 direction = (targetPos - spawnPos).normalized;

            GameObject acid = Instantiate(acidProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));

            if (acid.TryGetComponent<Rigidbody>(out Rigidbody rb))
                rb.AddForce(direction * spitForce, ForceMode.Impulse);
        }
    }
}