using UnityEngine;

public class AudioGate : MonoBehaviour
{
    [SerializeField] private bool startMuted = true;

    private void Start()
    {
        AudioListener.volume = startMuted ? 0f : 1f;
    }

    public void SetMuted(bool muted)
    {
        AudioListener.volume = muted ? 0f : 1f;
    }
}
