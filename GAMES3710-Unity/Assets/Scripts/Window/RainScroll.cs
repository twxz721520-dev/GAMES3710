using UnityEngine;

public class RainScroll : MonoBehaviour
{
    public Renderer targetRenderer;

    [Range(0f, 0.2f)]
    public float speed = 0.02f;

    // 勾上就把方向翻转
    public bool invertDirection = false;

    // 这里填你要滚动的贴图槽，默认 BaseMap
    public string textureProperty = "_BaseMap";

    Vector2 offset;

    void Start()
    {
        if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer == null) return;

        offset = targetRenderer.material.GetTextureOffset(textureProperty);
    }

    void Update()
    {
        if (targetRenderer == null) return;

        float dir = invertDirection ? 1f : -1f;
        offset.y += dir * speed * Time.deltaTime;

        targetRenderer.material.SetTextureOffset(textureProperty, offset);
    }
}