using UnityEngine;
using System.Collections;

public class TraderNPC : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float triggerDistance = 4f;

    [Header("Audio")]
    [SerializeField] private AudioClip standUpSound;
    [SerializeField] private AudioClip sitDownSound;
    [SerializeField] private AudioClip greetingVoiceline;
    [SerializeField] private float voicelineDelay = 1.2f;

    private Animator animator;
    private AudioSource audioSource;
    private Transform player;
    private bool hasStoodUp = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool playerNear = dist <= triggerDistance;

        animator.SetBool("PlayerNear", playerNear);

        if (playerNear && !hasStoodUp)
        {
            hasStoodUp = true;
            StartCoroutine(PlayStandUpAudio());
        }

        if (!playerNear && hasStoodUp)
        {
            hasStoodUp = false;
            StartCoroutine(PlaySitDownAudio());
        }
    }

    private IEnumerator PlayStandUpAudio()
    {
        if (standUpSound != null)
            audioSource.PlayOneShot(standUpSound, 1f);

        yield return new WaitForSeconds(voicelineDelay);

        if (greetingVoiceline != null)
            audioSource.PlayOneShot(greetingVoiceline, 1f);
    }

    private IEnumerator PlaySitDownAudio()
    {
        if (sitDownSound != null)
            audioSource.PlayOneShot(sitDownSound, 1f);

        yield return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}