using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Contracts;

public interface ITargetFabric
{
    public ITargetObserver CreateTargetObserver(IElement element,int propertyId, Action<int, object> onPropertyChanged);
}
