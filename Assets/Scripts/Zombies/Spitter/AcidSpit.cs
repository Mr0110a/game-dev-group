using UnityEngine;

public class AcidSpit : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifeTime = 5f;

    private bool hasHit = false;

    void Start()
    {
        Destroy(gameObject, lifeTime);

        // Ignore collision with all zombies so it doesn't self-destruct on spawn
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
        Collider myCollider = GetComponent<Collider>();
        foreach (GameObject zombie in zombies)
        {
            Collider zombieCol = zombie.GetComponent<Collider>();
            if (zombieCol != null)
                Physics.IgnoreCollision(myCollider, zombieCol);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (hasHit) return;
        hasHit = true;

        if (other.transform.CompareTag("Player"))
        {
            // other.transform.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}