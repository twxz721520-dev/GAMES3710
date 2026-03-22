using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectiveTrigger : MonoBehaviour
{
    public enum TriggerMode
    {
        OnPickup,
        OnDoorActivated,
        OnPlayerEnter,
        OnKeyPress,
        Manual
    }

    [Header("Objective")]
    [SerializeField] private string subTaskId;

    [Header("Trigger")]
    [SerializeField] private TriggerMode mode = TriggerMode.Manual;
    [SerializeField] private bool completeObjective;

    [Header("Long-term Goal (optional, updates on trigger)")]
    [SerializeField] private string newLongTermGoal;

    [Header("Key Press (only for OnKeyPress mode)")]
    [SerializeField] private Key[] keys;

    private bool _triggered;

    private void Start()
    {
        if (mode == TriggerMode.OnPickup)
        {
            if (GetComponent<ItemPickup>() == null && GetComponent<PillPickup>() == null)
                Debug.LogWarning($"ObjectiveTrigger: OnPickup mode but no ItemPickup/PillPickup on {name}");
        }
        else if (mode == TriggerMode.OnDoorActivated)
        {
            if (GetComponent<LockedDoor>() == null)
                Debug.LogWarning($"ObjectiveTrigger: OnDoorActivated mode but no LockedDoor on {name}");
        }
    }

    private void LateUpdate()
    {
        if (_triggered) return;

        switch (mode)
        {
            case TriggerMode.OnDoorActivated:
                var door = GetComponent<LockedDoor>();
                if (door != null && door.isOpen)
                    Fire();
                break;
            case TriggerMode.OnKeyPress:
                if (keys != null)
                {
                    foreach (var k in keys)
                    {
                        if (k != Key.None && Keyboard.current[k].wasPressedThisFrame)
                        {
                            Fire();
                            break;
                        }
                    }
                }
                break;
        }
    }

    // Handles both ItemPickup (SetActive false) and PillPickup (Destroy)
    private void OnDisable()
    {
        if (_triggered) return;
        if (mode != TriggerMode.OnPickup) return;

        var item = GetComponent<ItemPickup>();
        if (item != null && item.isPickedUp)
        {
            Fire();
            return;
        }

        var pill = GetComponent<PillPickup>();
        if (pill != null)
        {
            Fire();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (mode != TriggerMode.OnPlayerEnter) return;

        if (other.CompareTag("Player"))
            Fire();
    }

    /// <summary>
    /// Call from Animation Events, Timeline signals, or any external script.
    /// </summary>
    public void Complete()
    {
        if (!_triggered)
            Fire();
    }

    private void Fire()
    {
        _triggered = true;

        if (ObjectiveManager.Instance == null) return;

        if (!string.IsNullOrEmpty(newLongTermGoal))
            ObjectiveManager.Instance.SetLongTermGoal(newLongTermGoal);

        if (!string.IsNullOrEmpty(subTaskId))
            ObjectiveManager.Instance.CompleteSubTask(subTaskId);

        if (completeObjective)
            ObjectiveManager.Instance.CompleteCurrentObjective();
    }
}
