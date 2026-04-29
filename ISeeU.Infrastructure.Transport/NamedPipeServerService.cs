using System.IO.Pipes;
using System.Text;
using ISeeU.Application.Contracts;

namespace ISeeU.Infrastructure.Transport;

public class NamedPipeServerService(string pipeName) : ICommunicationChannel
{
    
    private readonly string _pipeName = pipeName;
    private Action<string> _onMessageReceived;
    private CancellationTokenSource _cts;
    
    
    public void StartListening(Action<string> onMessageReceived)
    {
        _onMessageReceived = onMessageReceived;
        _cts = new CancellationTokenSource();
        Task.Run( () => ListenLoop(_cts.Token));
    }

    private async Task ListenLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var server = new NamedPipeServerStream(_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            await server.WaitForConnectionAsync(ct);

            var buffer = new byte[1024];
            var bytesRead = await server.ReadAsync(buffer, ct);
            var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            _onMessageReceived?.Invoke(message);

            server.Disconnect();
            server.Close(); 
        }
    }
    
    
    public void StopListening()
    {
        _cts?.Cancel();
    }
}