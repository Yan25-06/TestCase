using UnityEngine;

public class CTAController : MonoBehaviour
{
    [SerializeField] private GameObject ctaRoot;

    public void ShowCTA()
    {
        if (ctaRoot != null)
        {
            ctaRoot.SetActive(true);
        }
    }

    public void HideCTA()
    {
        if (ctaRoot != null)
        {
            ctaRoot.SetActive(false);
        }
    }
}
