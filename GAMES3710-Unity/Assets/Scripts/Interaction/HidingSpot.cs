using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class HidingSpot : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject promptUI;
    public string enterPrompt = "Press E to hide";
    public string exitPrompt = "Press E to exit";

    private bool _playerInRange;
    private bool _isPlayerHiding;
    private TMP_Text _promptText;

    private void Start()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
            _promptText = promptUI.GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        if (_playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        _isPlayerHiding = !_isPlayerHiding;

        if (PlayerHideState.Instance != null)
        {
            PlayerHideState.Instance.SetHiding(_isPlayerHiding);
        }

        UpdatePromptText();
    }

    private void UpdatePromptText()
    {
        if (_promptText != null)
        {
            _promptText.text = _isPlayerHiding ? exitPrompt : enterPrompt;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            if (promptUI != null)
            {
                promptUI.SetActive(true);
                UpdatePromptText();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            if (promptUI != null)
            {
                promptUI.SetActive(false);
            }

            if (_isPlayerHiding)
            {
                _isPlayerHiding = false;
                if (PlayerHideState.Instance != null)
                {
                    PlayerHideState.Instance.SetHiding(false);
                }
            }
        }
    }
}
