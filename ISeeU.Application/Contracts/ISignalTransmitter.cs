namespace ISeeU.Application.Contracts;

public interface ISignalTransmitter
{
    void Send(string message);
}