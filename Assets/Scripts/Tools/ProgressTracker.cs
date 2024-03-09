using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic class for tracking progress on a list of items.
/// </summary>
/// <typeparam name="T">Type of items being tracked.</typeparam>
[Serializable]
public class ProgressTracker<T>
{
    [SerializeField] private List<T> itemList;
    [SerializeField] private List<bool> executionStatusList;
    [SerializeField] private Func<T, bool> executionRequirements;
    [SerializeField] private Action<T> executionActions;

    /// <summary>
    /// Constructor for ProgressTracker class.
    /// </summary>
    /// <param name="itemList">List of items to track progress on.</param>
    /// <param name="executionRequirements">Function defining the execution requirements for each item.</param>
    /// <param name="executionActions">Action to be executed upon meeting the requirements for each item.</param>
    public ProgressTracker(List<T> itemList, Func<T, bool> executionRequirements, Action<T> executionActions)
    {
        executionStatusList = new();
        Initialize(itemList, executionRequirements, executionActions);
    }

    /// <summary>
    /// Default constructor for ProgressTracker class.
    /// </summary>
    public ProgressTracker()
    {
        executionStatusList = new();
    }

    /// <summary>
    /// Update method to check and execute actions based on progress.
    /// </summary>
    /// <remarks>
    /// This method needs to be called within the Update() function of a MonoBehaviour script in Unity.
    /// </remarks>
    public void Update()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            if (!executionStatusList[i] && executionRequirements(itemList[i]))
            {
                executionActions(itemList[i]);
                UpdateExecutionStatus(i);
            }
        }
    }

    /// <summary>
    /// Initializes the progress tracker with provided parameters.
    /// </summary>
    /// <param name="itemList">List of items to track progress on.</param>
    /// <param name="executionRequirements">Function defining the execution requirements for each item.</param>
    /// <param name="executionActions">Action to be executed upon meeting the requirements for each item.</param>
    public void Initialize(List<T> itemList, Func<T, bool> executionRequirements, Action<T> executionActions)
    {
        this.itemList = itemList;
        this.executionRequirements = executionRequirements;
        this.executionActions = executionActions;
        InitializeExecutionStatusList();
    }

    /// <summary>
    /// Initializes the execution status list with default values.
    /// </summary>
    private void InitializeExecutionStatusList()
    {
        executionStatusList.Clear();
        for (int i = 0; i < itemList.Count; i++)
        {
            executionStatusList.Add(false);
        }
    }

    /// <summary>
    /// Updates the execution status of an item at the specified index.
    /// </summary>
    /// <param name="index">Index of the item to update.</param>
    private void UpdateExecutionStatus(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            executionStatusList[index] = true;
        }
        else
        {
            throw new IndexOutOfRangeException("Index is out of range.");
        }
    }
}
