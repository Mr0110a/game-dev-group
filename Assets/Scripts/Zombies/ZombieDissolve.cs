using UnityEngine;
using System.Collections;

public class ZombieDissolve : MonoBehaviour
{
    [SerializeField] private float dissolveDelay = 30f;
    [SerializeField] private float dissolveDuration = 5f;

    private Renderer[] renderers;

    // Called by ZombieBase when zombie dies
    public void StartDissolve()
    {
        renderers = GetComponentsInChildren<Renderer>();
        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        yield return new WaitForSeconds(dissolveDelay);

        foreach (Renderer r in renderers)
        {
            foreach (Material mat in r.materials)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
        }

        float timer = 0f;
        while (timer < dissolveDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / dissolveDuration);

            foreach (Renderer r in renderers)
                foreach (Material mat in r.materials)
                {
                    Color col = mat.color;
                    col.a = alpha;
                    mat.color = col;
                }

            yield return null;
        }

        Destroy(gameObject);
    }
}