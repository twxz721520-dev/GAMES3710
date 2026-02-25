using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanityUI : MonoBehaviour
{
    [Header("References")]
    public Slider sanitySlider;
    public TMP_Text pillCountText;

    [Header("Display")]
    [SerializeField] private string sanityLabel = "Sanity";
    [SerializeField] private string pillLabel = "Pills";
    [SerializeField] private Color fillColor = new Color(0.3f, 0.8f, 0.5f, 1f);
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);

    private void Start()
    {
        SetupSliderStyle();
        CreateSanityLabel();

        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged += UpdateSanityDisplay;
            SanityManager.Instance.OnPillCountChanged += UpdatePillCount;

            UpdateSanityDisplay();
            UpdatePillCount();
        }
    }

    private void OnDestroy()
    {
        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.OnSanityChanged -= UpdateSanityDisplay;
            SanityManager.Instance.OnPillCountChanged -= UpdatePillCount;
        }
    }

    private void SetupSliderStyle()
    {
        if (sanitySlider == null) return;

        // Hide handle
        if (sanitySlider.handleRect != null)
            sanitySlider.handleRect.gameObject.SetActive(false);

        // Set background color
        var background = sanitySlider.transform.Find("Background");
        if (background != null)
        {
            var bgImage = background.GetComponent<Image>();
            if (bgImage != null)
                bgImage.color = backgroundColor;
        }

        // Set fill color
        if (sanitySlider.fillRect != null)
        {
            var fillImage = sanitySlider.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = fillColor;
        }
    }

    private void CreateSanityLabel()
    {
        if (sanitySlider == null) return;

        var labelObj = new GameObject("SanityLabel");
        labelObj.transform.SetParent(sanitySlider.transform.parent, false);
        labelObj.transform.SetSiblingIndex(sanitySlider.transform.GetSiblingIndex());

        var rectTransform = labelObj.AddComponent<RectTransform>();
        var sliderRect = sanitySlider.GetComponent<RectTransform>();

        // Anchor to same position as slider but shifted above
        rectTransform.anchorMin = sliderRect.anchorMin;
        rectTransform.anchorMax = sliderRect.anchorMax;
        rectTransform.pivot = new Vector2(0f, 0f);
        rectTransform.anchoredPosition = new Vector2(
            sliderRect.anchoredPosition.x - sliderRect.sizeDelta.x * sliderRect.pivot.x,
            sliderRect.anchoredPosition.y + sliderRect.sizeDelta.y * (1f - sliderRect.pivot.y)
        );
        rectTransform.sizeDelta = new Vector2(sliderRect.sizeDelta.x, 24f);

        var text = labelObj.AddComponent<TextMeshProUGUI>();
        text.text = sanityLabel;
        text.fontSize = 24f;
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;
    }

    private void UpdateSanityDisplay()
    {
        if (sanitySlider != null && SanityManager.Instance != null)
        {
            sanitySlider.value = SanityManager.Instance.GetSanityNormalized();
        }
    }

    private void UpdatePillCount()
    {
        if (pillCountText != null && SanityManager.Instance != null)
        {
            pillCountText.text = $"{pillLabel}: {SanityManager.Instance.pillCount}";
        }
    }
}
