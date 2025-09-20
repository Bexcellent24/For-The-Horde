using UnityEngine;

public class ClickMarkerFade : MonoBehaviour
{
    public float lifeTime = .5f;
    public float fadeDuration = 0.25f;

    private Material mat;
    private Color originalColor;
    private float timer;

    void Start()
    {
        // clone the material so each marker fades independently
        Renderer rend = GetComponent<Renderer>();
        mat = rend.material;
        originalColor = mat.color;

        timer = lifeTime;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= fadeDuration)
        {
            float t = Mathf.Clamp01(timer / fadeDuration);
            Color c = originalColor;
            c.a = t; // scale alpha
            mat.color = c;
        }

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}