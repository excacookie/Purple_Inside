using UnityEngine;

namespace Magic;

internal class DestroyAfterTime : MonoBehaviour
{
    
    private float _duration;

    public void SetDuration(float time)
    {
        _duration = time;
        Destroy(gameObject, _duration);
    }

}