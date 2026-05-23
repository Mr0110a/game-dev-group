using UnityEngine;

public class ExplosionLight : MonoBehaviour
{
    private Light pointLight;
    private float startIntensity;
    [SerializeField] private float fadeDuration = 0.4f;
    private float timer = 0f;

    void Start()
    {
        pointLight = GetComponent<Light>();
        startIntensity = pointLight.intensity;
    }

    void Update()
    {
        timer += Time.deltaTime;
        pointLight.intensity = Mathf.Lerp(startIntensity, 0f, timer / fadeDuration);
        if (timer >= fadeDuration)
            Destroy(gameObject);
    }
}