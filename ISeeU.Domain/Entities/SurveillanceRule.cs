using ISeeU.Domain.Interfaces;

namespace ISeeU.Domain.Entities;

public class SurveillanceRule(int propertyId, TargetInfo target, Action<int, object> callback, ITargetObserver targetObs)
{
    public ITargetObserver Observer { get; } = targetObs;
    public int PropertyId { get; } = propertyId;
    public TargetInfo Target { get; } = target;
    public Action<int, object> Callback { get; } = callback;
}

