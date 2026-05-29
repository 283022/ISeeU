namespace ISeeU.Application.Contracts;

public interface ICommunicationChannel
{
    void StartListening(Func<string,string> onMessageReceived);
    void StopListening();
    void Send(string message);
}