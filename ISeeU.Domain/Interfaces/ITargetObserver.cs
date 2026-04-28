namespace ISeeU.Domain.Interfaces;

public interface ITargetObserver
{
    void Start(IElement element);
    void Stop();
}