using UnityEngine;
using System.Collections;

public class ExploderZombie : ZombieBase
{
    [Header("Exploder Specific")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionDamage = 50f;
    [SerializeField] private AudioClip explosionSound;
    

    [Header("Tweak Settings")]
    [SerializeField] private float fallDuration = 1.5f;    // match this to your fall animation clip length
    [SerializeField] private float fallSoundDelay = 0f; // delay in seconds before thud plays
    [SerializeField] private float tweakDuration = 2.5f;   // how long he convulses on ground
    [SerializeField] private AudioClip tweakSound;         // looping buzz/growl during convulse

    private bool isExploding = false;

    protected override void Die(Vector3 damageSourcePosition)
    {
        if (isExploding) return;
        isExploding = true;

        currentState = State.Dead;
        agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        StartCoroutine(FallTweakExplode());
    }

    private IEnumerator FallTweakExplode()
    {
        // Phase 1 — Fall to ground
        animator.SetTrigger("StartFall");
        if (fallSoundDelay > 0f)
            yield return new WaitForSeconds(fallSoundDelay);
        PlaySound(deathClip, 1f);
        yield return new WaitForSeconds(fallDuration - fallSoundDelay);

        // Phase 2 — Convulse on ground
        // Animator auto transitions Fall -> Tweak via Exit Time
        if (tweakSound != null)
        {
            audioSource.clip = tweakSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        yield return new WaitForSeconds(tweakDuration);

        // Phase 3 — Explode, body gone instantly
        audioSource.Stop();
        audioSource.loop = false;
        Explode();
    }

    private void Explode()
    {
        // Spawn explosion prefab
        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        // Play explosion sound independently so it survives object destruction
        if (explosionSound != null)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);

        // Body gone instantly — no corpse since he exploded
        Destroy(gameObject);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
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