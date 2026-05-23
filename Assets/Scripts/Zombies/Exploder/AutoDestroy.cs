using UnityEngine;
public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float delay = 3f;
    void Start() => Destroy(gameObject, delay);
}