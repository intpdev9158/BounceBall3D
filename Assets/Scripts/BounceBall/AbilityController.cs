using UnityEngine;

public enum AbilityType
{
    None = 0,
    AirBoost = 1,
    UpBoost = 2,
}

public class AbilityController : MonoBehaviour
{   
    public AbilityType Current { get; private set; } = AbilityType.None;

    AirBoostController airBoost;
    UpBoostController upBoost;

    void Awake()
    {
        airBoost = GetComponent<AirBoostController>();
        upBoost  = GetComponent<UpBoostController>();
    }

    public void SetAbility(AbilityType type)
    {
        if (type == Current) return;

        DisableCurrent();
        Current = type;
        EnableCurrent();
    }

    void DisableCurrent()
    {
        switch (Current)
        {
            case AbilityType.AirBoost:
                if (airBoost) airBoost.SetEnabled(false);
                break;
            case AbilityType.UpBoost:
                if (upBoost) upBoost.SetEnabled(false);
                break;
        }
    }

    void EnableCurrent()
    {
        switch (Current)
        {
            case AbilityType.AirBoost:
                if (airBoost) airBoost.SetEnabled(true);
                break;
            case AbilityType.UpBoost:
                if (upBoost) upBoost.SetEnabled(true);
                break;
        }
    }
}
