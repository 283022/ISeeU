namespace ISeeU.Application.Contracts;

public interface ITargetObserver
{
    void Start(IElement element);
    void Stop();
}