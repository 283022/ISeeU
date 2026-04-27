using ISeeU.Application.CommandHandlers;
using ISeeU.Domain.Entities;

namespace ISeeU.Application.Services;

public class SurveillanceManager
{
    private List<SurveillanceRule> _surveillanceRules;

    public void Subscribe(SurveillanceRule surveillanceRule)
    {
        _surveillanceRules.Add(surveillanceRule);
    }

    public void UnSubscribe(SurveillanceRule surveillanceRule)
    {
        _surveillanceRules.Remove(surveillanceRule);
    }
}