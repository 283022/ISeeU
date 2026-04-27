using Interop.UIAutomationClient;

namespace ISeeU.Application.Contracts;

public interface ITargetObserver
{
    void Start(string name, int propertyId);
    void Stop();
}