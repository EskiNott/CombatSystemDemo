using System;
using UnityEngine;

[Serializable]
public class Timer
{
    /// <summary>
    /// Whether the timer is currently running or not
    /// </summary>
    [field: SerializeField] public bool Running { get; private set; }

    /// <summary>
    /// The total time duration for the timer
    /// </summary>
    [SerializeField] float releaseTime;

    /// <summary>
    /// The elapsed time since the timer started running
    /// </summary>
    [SerializeField] float runningTime;

    [field: SerializeField] public TimerMode Mode { get; private set; }

    /// <summary>
    /// Event invoked when the timer is started
    /// </summary>
    public event Action TimerStarted;

    /// <summary>
    /// Event invoked when the timer is played (started or resumed)
    /// </summary>
    public event Action TimerPlayed;

    /// <summary>
    /// Event invoked when the timer is paused
    /// </summary>
    public event Action TimerPaused;

    /// <summary>
    /// Event invoked when the timer ends
    /// </summary>
    public event Action TimerEnded;

    /// <summary>
    /// Event invoked when the timer is running (updated)
    /// </summary>
    public event Action TimerRunning;

    /// <summary>
    /// Event invoked when the timer is reset
    /// </summary>
    public event Action TimerReset;

    /// <summary>
    /// Constructor for the Timer class
    /// </summary>
    public Timer()
    {
        Running = false;
        releaseTime = 0;
        runningTime = 0;
        Mode = TimerMode.InstantStop;
    }

    /// <summary>
    /// Enum representing different modes of operation for the timer.
    /// </summary>
    [System.Serializable]
    public enum TimerMode
    {
        /// <summary>
        /// The timer operates in continuous mode, where it continues counting even after reaching the specified time.
        /// </summary>
        Continuous,

        /// <summary>
        /// The timer operates in instant stop mode, where it stops immediately upon reaching the specified time.
        /// </summary>
        InstantStop,

        /// <summary>
        /// The timer operates in loop mode, where it restarts the counting cycle when reaching the specified time.
        /// </summary>
        Loop
    }


    /// <summary>
    /// Get the progress of the timer.
    /// </summary>
    /// <returns>Representing the normalized time in float.</returns>
    public float Progress()
    {
        return releaseTime != 0 && runningTime != 0
        ? runningTime / releaseTime
        : 0;

    }

    /// <summary>
    /// Check if the timer has reached a certain progress.
    /// </summary>
    /// <param name="Time">The time to check against.</param>
    /// <returns>True if the timer has reached the specified time, false otherwise.</returns>
    public bool ReachProgress(float Time)
    {
        return runningTime > Time;
    }

    /// <summary>
    /// Checks if the timer has reached a certain progress percentage.
    /// </summary>
    /// <param name="percentage">The percentage value (between 0 and 1) representing the progress to check against.</param>
    /// <returns>True if the timer has reached the specified progress percentage, false otherwise.</returns>
    /// <remarks>Typically, the <paramref name="percentage"/> parameter should be in the range of 0 to 1, where 0 represents 0% progress and 1 represents 100% progress.</remarks>
    public bool ReachProgressPercentage(float percentage)
    {
        return runningTime > releaseTime * percentage;
    }

    /// <summary>
    /// Reset the timer to its initial state
    /// </summary>
    public void Reset()
    {
        releaseTime = 0;
        ResetProgress();
        SetTimerMode(TimerMode.InstantStop);
        TimerReset?.Invoke();
    }

    /// <summary>
    /// Start or resume the timer
    /// </summary>
    public void Play()
    {
        Running = true;
        TimerPlayed?.Invoke();
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void Pause()
    {
        Running = false;
        TimerPaused?.Invoke();
    }

    /// <summary>
    /// Initialize the timer with the specified release time and start it
    /// </summary>
    /// <param name="releaseTime"></param>
    public void Begin(float releaseTime, TimerMode timerMode = TimerMode.InstantStop)
    {
        Reset();
        this.releaseTime = releaseTime;
        SetTimerMode(timerMode);
        Play();
        TimerStarted?.Invoke();
    }

    /// <summary>
    /// Reset the timer's progress to zero
    /// </summary>
    public void ResetProgress()
    {
        runningTime = 0;
        Running = false;
    }

    /// <summary>
    /// Manually set the progress of the timer in seconds
    /// </summary>
    /// <param name="seconds"></param>
    public void SetProgress(float seconds)
    {
        runningTime = seconds;
    }

    /// <summary>
    /// Sets the progress of the timer based on a percentage value.
    /// </summary>
    /// <param name="percentage">The percentage value (between 0 and 1) representing the progress of the timer.</param>
    /// <remarks>Typically, the <paramref name="percentage"/> parameter should be in the range of 0 to 1, where 0 represents 0% progress and 1 represents 100% progress.</remarks>
    public void SetProgressPercentage(float percentage)
    {
        runningTime = releaseTime * percentage;
    }


    /// <summary>
    /// Sets the mode of operation for the timer.
    /// </summary>
    /// <param name="timerMode">The mode to set for the timer.</param>
    public void SetTimerMode(TimerMode timerMode)
    {
        this.Mode = timerMode;
    }

    /// <summary>
    /// Check if the timer has reached its end
    /// </summary>
    /// <returns></returns>
    public bool IsEnd()
    {
        return runningTime > releaseTime;
    }

    /// <summary>
    /// Update the timer's running time, invoke events, and check for timer completion.
    /// Ensure to call Timer's Update method in Unity's Update() method to update the timer.
    /// </summary>
    public void Update()
    {
        if (!Running) { return; }
        runningTime += Time.deltaTime;
        if (IsEnd())
        {
            TimerEnded?.Invoke();
            if (Mode == TimerMode.InstantStop)
            {
                Pause();
                return;
            }
            else if (Mode == TimerMode.Loop)
            {
                runningTime -= releaseTime;
                runningTime = runningTime > 0 ? runningTime : 0;
                TimerStarted?.Invoke();
            }
        }
        TimerRunning?.Invoke();
    }
}