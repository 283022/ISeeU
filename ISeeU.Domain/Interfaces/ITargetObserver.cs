namespace ISeeU.Domain.Interfaces;

public interface ITargetObserver
{
    void Start(string name, int propertyId);
    void Stop();
}