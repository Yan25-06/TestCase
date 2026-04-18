using UnityEngine;

public class GameOverPanelController : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleStateChanged;
            HandleStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleStateChanged;
        }
    }

    private void HandleStateChanged(GameState state)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(state == GameState.GameOver);
        }
    }
}
