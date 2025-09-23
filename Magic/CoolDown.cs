using System.Collections;
using MEC;

namespace Magic;

public struct CoolDown : IEnumerator<float>
{
    #region Properties & Variables
    internal float _coolDownTime;

    public float CurrentCooldown => _coolDownTime - Timing.LocalTime;

    public float Duration { get; }
    public bool IsActive => CurrentCooldown > 0;

    float IEnumerator<float>.Current => Math.Min(10 + Timing.LocalTime, _coolDownTime);
    object IEnumerator.Current => ((IEnumerator<float>)this).Current;
    #endregion

    #region Constructor & Destructor
    public CoolDown(float durationSecond)
    {
        Duration = durationSecond;
    }
    #endregion

    #region Methods
    /// <param name="message"><see cref="String.Empty"/> when <see langword="false"/></param>
    public bool NotAllow(out string message)
    {
        if (!IsActive)
        {
            message = string.Empty;
            return false;
        }

        message = Translation.Singleton.CoolDown.Replace("%time%", CurrentCooldown.ToString());
        return true;
    }

    public void Reset()
    {
        _coolDownTime = 0;
    }

    public void Start()
    {
        _coolDownTime = Timing.WaitForSeconds(Duration);
    }

    /// <summary>
    /// Use as <see langword="yield"/> <see langword="return"/> value for coroutine.
    /// </summary>
    public float WaitEnd() => _coolDownTime;

    void IDisposable.Dispose() { }

    bool IEnumerator.MoveNext() => IsActive;
    #endregion
}
