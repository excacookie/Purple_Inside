using System;
using UnityEngine;

namespace Magic;

public struct CoolDown { 

    #region Properties & Variables 
    private float _startTime; // Timestamp when cooldown started
    private float _endTime; // Timestamp when cooldown ends
    public float Duration { get; } // Cooldown duration in seconds
    public float CurrentCooldown => Mathf.Max(0, _endTime - Time.time); // Remaining cooldown time
    public bool IsActive => Time.time < _endTime; // True if cooldown is still active
    #endregion

    #region Constructor
    public CoolDown(float durationSeconds)
    {
        Duration = durationSeconds;
        _startTime = 0;
        _endTime = 0;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Checks if the cooldown prevents an action and provides a message if active.
    /// </summary>
    /// <param name="message">Empty string if cooldown is not active, otherwise a formatted message.</param>
    /// <returns>True if cooldown is active, false otherwise.</returns>
    public bool NotAllow(out string message)
    {
        if (!IsActive)
        {
            message = string.Empty;
            return false;
        }

        message = Translation.Singleton.CoolDown.Replace("%time%", CurrentCooldown.ToString("F2"));
        return true;
    }

    /// <summary>
    /// Resets the cooldown, making it inactive.
    /// </summary>
    public void Reset()
    {
        _startTime = 0;
        _endTime = 0;
    }

    /// <summary>
    /// Starts the cooldown, setting the start and end timestamps.
    /// </summary>
    public void Start()
    {
        _startTime = Time.time;
        _endTime = _startTime + Duration;
        Logger.Info($"Cooldown started: Duration = {Duration}, EndTime = {_endTime}");
    }

    /// <summary>
    /// Returns the remaining cooldown time for use in coroutines.
    /// </summary>
    public float WaitEnd()
    {
        return CurrentCooldown;
    }
}
#endregion