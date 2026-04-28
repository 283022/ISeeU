using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Contracts;

public interface ITargetFabric
{
    public ITargetObserver CreateTargetObserver(int propertyId, Action<int, object> onPropertyChanged);
}
