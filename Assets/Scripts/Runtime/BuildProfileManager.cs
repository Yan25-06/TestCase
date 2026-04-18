using UnityEngine;

public enum AdNetworkProfile
{
    IronSource,
    AppLovin,
    Mintegral
}

public class BuildProfileManager : MonoBehaviour
{
    [SerializeField] private AdNetworkProfile activeProfile = AdNetworkProfile.IronSource;

    public AdNetworkProfile ActiveProfile => activeProfile;
}
