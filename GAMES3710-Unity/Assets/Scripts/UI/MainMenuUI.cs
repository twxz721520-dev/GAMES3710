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

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;

    private void Start()
    {
        var rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        CreateUI();
    }

    private void CreateUI()
    {
        var bg = CreateChild("BlackBG", transform);
        StretchFull(bg);
        bg.AddComponent<Image>().color = Color.black;

        var titleObj = CreateChild("TitleBackground", bg.transform);
        StretchFull(titleObj);
        var titleImage = titleObj.AddComponent<Image>();
        titleImage.sprite = titleSprite;
        titleImage.raycastTarget = false;

        var fitter = titleObj.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        if (titleSprite != null)
            fitter.aspectRatio = titleSprite.rect.width / titleSprite.rect.height;

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

        CreateMenuButton("IntroductionBtn", container.transform, introductionNormal, introductionHovered, OnIntroduction);
        CreateMenuButton("StartBtn", container.transform, startNormal, startHovered, OnStart);
        CreateMenuButton("ExitBtn", container.transform, exitNormal, exitHovered, OnExit);
    }

    private void CreateMenuButton(string name, Transform parent, Sprite normal, Sprite hovered, UnityEngine.Events.UnityAction onClick)
    {
        var btnObj = CreateChild(name, parent);
        var rect = btnObj.GetComponent<RectTransform>();

        var image = btnObj.AddComponent<Image>();
        image.sprite = normal;
        image.preserveAspect = true;
        image.color = Color.white;
        image.type = Image.Type.Simple;

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

        button.onClick.AddListener(() =>
        {
            PlayClickSound();
            onClick.Invoke();
        });

        var trigger = btnObj.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((eventData) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }

    private void PlayClickSound()
    {
        Debug.Log("click sound called");

        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }

    private void PlayHoverSound()
    {
        Debug.Log("hover sound called");

        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    private void OnIntroduction()
    {
        Debug.Log("Introduction - not yet implemented");
    }

    private void OnStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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