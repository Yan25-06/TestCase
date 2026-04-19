using UnityEngine;

public class PlayableBootstrap : MonoBehaviour
{
    [SerializeField] private bool autoStartGame = false;

    private void Start()
    {
        if (!autoStartGame || GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.StartGame();
    }
}
