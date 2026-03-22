using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    [Header("Title")]
    [SerializeField] private Sprite titleSprite;

    [Header("Button Sprites")]
    [SerializeField] private Sprite introductionNormal;
    [SerializeField] private Sprite introductionHovered;
    [SerializeField] private Sprite startNormal;
    [SerializeField] private Sprite startHovered;
    [SerializeField] private Sprite exitNormal;
    [SerializeField] private Sprite exitHovered;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        CreateUI();
    }

    private void CreateUI()
    {
        // Black fullscreen background
        var bg = CreateChild("Background", transform);
        StretchFull(bg);
        bg.AddComponent<Image>().color = Color.black;

        // Title logo
        var titleObj = CreateChild("Title", bg.transform);
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -60f);

        var titleImage = titleObj.AddComponent<Image>();
        titleImage.sprite = titleSprite;
        titleImage.preserveAspect = true;
        titleImage.raycastTarget = false;
        titleImage.SetNativeSize();
        // Scale down title to reasonable size
        float titleScale = 0.5f;
        titleRect.sizeDelta = titleRect.sizeDelta * titleScale;

        // Button container
        var container = CreateChild("Buttons", bg.transform);
        var containerRect = container.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.35f);
        containerRect.anchorMax = new Vector2(0.5f, 0.35f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(400f, 300f);

        var layout = container.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 20f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        // Buttons
        CreateMenuButton("IntroductionBtn", container.transform,
            introductionNormal, introductionHovered, OnIntroduction);
        CreateMenuButton("StartBtn", container.transform,
            startNormal, startHovered, OnStart);
        CreateMenuButton("ExitBtn", container.transform,
            exitNormal, exitHovered, OnExit);
    }

    private void CreateMenuButton(string name, Transform parent,
        Sprite normal, Sprite hovered, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = CreateChild(name, parent);
        var rect = btnObj.GetComponent<RectTransform>();

        var image = btnObj.AddComponent<Image>();
        image.sprite = normal;
        image.preserveAspect = true;
        image.color = Color.white;
        image.type = Image.Type.Simple;

        // Size based on sprite native size, scaled down
        if (normal != null)
        {
            image.SetNativeSize();
            float scale = 0.5f;
            rect.sizeDelta = rect.sizeDelta * scale;
        }
        else
        {
            rect.sizeDelta = new Vector2(200f, 60f);
        }

        var button = btnObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;

        var spriteState = new SpriteState();
        spriteState.highlightedSprite = hovered;
        spriteState.selectedSprite = hovered;
        button.spriteState = spriteState;

        button.onClick.AddListener(onClick);
    }

    private void OnIntroduction()
    {
        // TODO: show introduction panel
        Debug.Log("Introduction - not yet implemented");
    }

    private void OnStart()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private GameObject CreateChild(string name, Transform parent)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<RectTransform>();
        return obj;
    }

    private void StretchFull(GameObject obj)
    {
        var rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
