using UnityEngine;
using UnityEngine.InputSystem;

public class PillPickup : Interactable
{
    [Header("Pill Settings")]
    public string displayName = "Pill";

    [Header("SFX")]
    public AudioClip pickupClip;
    [Range(0f, 1f)] public float pickupVolume = 0.8f;

    private void Update()
    {
        if (PlayerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Interact();
        }
    }

    public override void Interact()
    {
        if (SFXPlayer.Instance != null)
        {
            SFXPlayer.Instance.PlayOneShot(pickupClip, pickupVolume);
        }

        if (SanityManager.Instance != null)
        {
            SanityManager.Instance.AddPill();
        }

        if (PromptUI.Instance != null)
        {
            PromptUI.Instance.Show("Obtained " + displayName);
        }

        HidePrompt();
        Destroy(gameObject);
    }

    protected override void OnPlayerEnter()
    {
        ShowPrompt("Press E to pick up");
    }

    protected override void OnPlayerExit()
    {
        HidePrompt();
    }
}