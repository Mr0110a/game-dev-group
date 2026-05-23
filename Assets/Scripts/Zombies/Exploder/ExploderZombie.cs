using UnityEngine;

public class ExploderZombie : ZombieBase
{
    [Header("Exploder Specific")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private AudioClip heavyAttackSwingClip; // grunt/swing override
    [SerializeField] private AudioClip heavyAttackClip;      // impact override


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
        // Boom before calling base death
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);

        // Damage player if in range
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= explosionRadius)
            {
                // player.GetComponent<PlayerHealth>()?.TakeDamage(explosionDamage);
            }
        }

        base.Die();
    }
}
