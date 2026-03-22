using UnityEngine;
using System;
using System.Collections.Generic;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Serializable]
    public class SubTask
    {
        public string id;
        public string description;
        [HideInInspector] public bool completed;
    }

    [Serializable]
    public class Objective
    {
        public string description;
        public List<SubTask> subTasks = new List<SubTask>();
    }

    [Header("Long-term Goal")]
    [SerializeField] private string longTermGoal;

    [Header("Objective Chain (in order)")]
    [SerializeField] private List<Objective> objectives = new List<Objective>();

    public string LongTermGoal => longTermGoal;
    public Objective CurrentObjective => _currentIndex < objectives.Count ? objectives[_currentIndex] : null;
    public bool AllCompleted => _currentIndex >= objectives.Count;

    public event Action<Objective> OnObjectiveChanged;
    public event Action<SubTask> OnSubTaskCompleted;
    public event Action OnAllObjectivesCompleted;

    private int _currentIndex;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        _currentIndex = 0;
        if (CurrentObjective != null)
            OnObjectiveChanged?.Invoke(CurrentObjective);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Mark a sub-task as completed by its ID. Can be called from anywhere.
    /// </summary>
    public void CompleteSubTask(string subTaskId)
    {
        // Search all objectives so future sub-tasks can be pre-completed
        for (int i = _currentIndex; i < objectives.Count; i++)
        {
            foreach (var sub in objectives[i].subTasks)
            {
                if (sub.id == subTaskId && !sub.completed)
                {
                    sub.completed = true;

                    // Only fire UI event if it's the current objective
                    if (i == _currentIndex)
                    {
                        OnSubTaskCompleted?.Invoke(sub);
                        CheckObjectiveComplete();
                    }
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Force-complete the current objective and advance. Useful for objectives with no sub-tasks.
    /// </summary>
    public void CompleteCurrentObjective()
    {
        if (AllCompleted) return;
        AdvanceObjective();
    }

    /// <summary>
    /// Update the long-term goal text at runtime.
    /// </summary>
    public void SetLongTermGoal(string goal)
    {
        longTermGoal = goal;
        // Re-fire so UI updates
        if (CurrentObjective != null)
            OnObjectiveChanged?.Invoke(CurrentObjective);
    }

    private void CheckObjectiveComplete()
    {
        var objective = CurrentObjective;
        if (objective == null) return;

        // No sub-tasks means manual completion only
        if (objective.subTasks.Count == 0) return;

        foreach (var sub in objective.subTasks)
        {
            if (!sub.completed) return;
        }

        // All sub-tasks done
        AdvanceObjective();
    }

    private void AdvanceObjective()
    {
        _currentIndex++;

        if (AllCompleted)
        {
            OnAllObjectivesCompleted?.Invoke();
        }
        else
        {
            OnObjectiveChanged?.Invoke(CurrentObjective);
            // Check if the new objective was already pre-completed
            CheckObjectiveComplete();
        }
    }
}
