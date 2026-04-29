namespace ISeeU.Application.Contracts;

public interface ICommunicationChannel
{
    void StartListening(Action<string> onMessageReceived);
    void StopListening();
}