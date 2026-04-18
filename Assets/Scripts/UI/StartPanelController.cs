using UnityEngine;

public class StartPanelController : MonoBehaviour
{
    public void OnTapStart()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }
}
