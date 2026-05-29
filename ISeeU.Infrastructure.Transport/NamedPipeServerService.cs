using System.IO.Pipes;
using System.Text;
using ISeeU.Application.Contracts;

namespace ISeeU.Infrastructure.Transport;

public class NamedPipeServerService : ICommunicationChannel
{
    private readonly string _pipeName;
    private Func<string, string>? _onMessageReceived;
    private CancellationTokenSource? _cts;
    private readonly object _lockObject = new();
    private bool _isRunning;
    private Task? _serverTask;

    public NamedPipeServerService(string pipeName)
    {
        _pipeName = pipeName;
    }

    public void StartListening(Func<string, string> onMessageReceived)
    {
        _onMessageReceived = onMessageReceived;
        _cts = new CancellationTokenSource();
        _isRunning = true;
        
        // Используем Task вместо Thread
        _serverTask = Task.Run(() => RunServerAsync(_cts.Token));
    }

    private async Task RunServerAsync(CancellationToken token)
    {
        Console.WriteLine("[Pipe] Server started");
        
        while (!token.IsCancellationRequested && _isRunning)
        {
            try
            {
                using (var pipeServer = new NamedPipeServerStream(
                    _pipeName, 
                    PipeDirection.InOut, 
                    1, 
                    PipeTransmissionMode.Message, 
                    PipeOptions.Asynchronous))
                {
                    Console.WriteLine("[Pipe] Waiting for connection...");
                    
                    // Асинхронное ожидание подключения
                    await pipeServer.WaitForConnectionAsync(token);
                    Console.WriteLine("[Pipe] Client connected!");
                    
                    // Обрабатываем клиента
                    await HandleClientAsync(pipeServer, token);
                }
                
                Console.WriteLine("[Pipe] Server instance disposed, ready for new connection");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Pipe] Server cancelled");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Pipe] Server error: {ex.Message}");
                if (_isRunning && !token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                }
            }
        }
        
        Console.WriteLine("[Pipe] Server stopped");
    }

    private async Task HandleClientAsync(NamedPipeServerStream clientPipe, CancellationToken token)
    {
        var buffer = new byte[4096];
        
        try
        {
            while (clientPipe.IsConnected && !token.IsCancellationRequested)
            {
                try
                {
                    using var memoryStream = new MemoryStream();
                    
                    // Асинхронно читаем сообщение полностью
                    do
                    {
                        var bytesRead = await clientPipe.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead == 0)
                        {
                            Console.WriteLine("[Pipe] Zero bytes read, client disconnected");
                            return;
                        }
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                    while (!clientPipe.IsMessageComplete);
                    
                    var message = Encoding.UTF8.GetString(memoryStream.ToArray());
                    Console.WriteLine($"[Pipe] Received: {message}");
                    
                    // Обрабатываем сообщение
                    string response;
                    lock (_lockObject)
                    {
                        response = _onMessageReceived?.Invoke(message) ?? "error|no_handler";
                    }
                    Console.WriteLine($"[Pipe] Response: {response}");
                    
                    // Асинхронно отправляем ответ
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await clientPipe.WriteAsync(responseBytes, 0, responseBytes.Length, token);
                    await clientPipe.FlushAsync(token);
                    Console.WriteLine("[Pipe] Response sent");
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("[Pipe] Operation cancelled");
                    break;
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"[Pipe] IO error: {ex.Message}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pipe] Client handler error: {ex.Message}");
        }
        
        Console.WriteLine("[Pipe] Client handler finished");
    }

    public void Send(string message)
    {
        Console.WriteLine($"[Pipe] Send: {message}");
    }

    public void StopListening()
    {
        Console.WriteLine("[Pipe] Stopping server...");
        _isRunning = false;
        _cts?.Cancel();
        
        try
        {
            _serverTask?.Wait(3000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Pipe] Stop error: {ex.Message}");
        }
        
        Console.WriteLine("[Pipe] Server stopped");
    }

    public void Dispose()
    {
        StopListening();
        _cts?.Dispose();
    }
}