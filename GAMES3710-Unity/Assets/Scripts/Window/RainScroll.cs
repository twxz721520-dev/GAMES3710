using UnityEngine;

public class RainScroll : MonoBehaviour
{
    public Renderer targetRenderer;
    public float speed = 0.02f;
    public bool invertDirection = false;

    Material mat;
    float y;

    void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null) return;

        mat = targetRenderer.material;

        y = mat.GetTextureOffset("_BaseMap").y;
    }

    void Update()
    {
        if (mat == null) return;

        float dir = invertDirection ? 1f : -1f;
        y += dir * speed * Time.deltaTime;

        y = Mathf.Repeat(y, 1f);

        mat.SetTextureOffset("_BaseMap", new Vector2(0f, y));
    }
}