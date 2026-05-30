using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Wpf;

public class Connection(Action<string> OnMessageReceived)
{
    //Данный класс отвечает за работу с запросами от клиента к серверу и от сервера к клиенту
    
    //используется межпроцессерное взаимодействие
    private readonly object _pipeLock = new object();
    private NamedPipeClientStream? _pipeClient;
    
    private readonly ConcurrentQueue<string> _sendQueue = new();
    private CancellationTokenSource? _cts;
    
    private readonly Action<string> _onMessageReceivedAction = OnMessageReceived;
    
    private Task? _readTask;
    private Task? _writeTask;
    
    public async Task ConnectToServiceAsync()
    {
        //пытаемся установить коннект с общим портом
        Console.WriteLine($"[CLIENT] Connecting to pipe: {ConnectInfo.ConnectInfo.PipeName}");
        try
        {_pipeClient = new NamedPipeClientStream(".", ConnectInfo.ConnectInfo.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            
            await _pipeClient.ConnectAsync(5000); 
            _pipeClient.ReadMode = PipeTransmissionMode.Message;
            
            Console.WriteLine("[CLIENT] Connected to service successfully");
            _onMessageReceivedAction("connected|");
            _cts = new CancellationTokenSource();
            
            // Запускаем чтение и запись в отдельных задачах
            _readTask = Task.Run(() => ReadMessagesLoop(_cts.Token));
            _writeTask = Task.Run(() => WriteMessagesLoop(_cts.Token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT] Connection error: {ex.Message}");
        }
    }

    public void OnClosed()
    {
        _cts?.Cancel();
        _pipeClient?.Close();
        _pipeClient?.Dispose();
    }

    public void Send(string message)
    {
        _sendQueue.Enqueue(message);
    }
    
    
    private async Task ReadMessagesLoop(CancellationToken token)
    {
        Console.WriteLine("[CLIENT] Read loop started");
        var buffer = new byte[65555];
        
        try
        {
            while (!token.IsCancellationRequested && _pipeClient?.IsConnected == true)
            {
                using var memoryStream = new MemoryStream();
                
                try
                {
                    // Читаем полное сообщение
                    do
                    {
                        var bytesRead = await _pipeClient.ReadAsync(buffer, 0, buffer.Length, token);
                        if (bytesRead == 0) break;
                        memoryStream.Write(buffer, 0, bytesRead);
                    }
                    while (!_pipeClient.IsMessageComplete);
                    
                    if (memoryStream.Length > 0)
                    {
                        var message = Encoding.UTF8.GetString(memoryStream.ToArray() as byte[]);
                        
                        //вот тут должен быть ПАРСЕР или вызов 
                        _onMessageReceivedAction(message);
                        Console.WriteLine($"[CLIENT] Received: {message}");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (IOException)
                {
                    Console.WriteLine("[CLIENT] Connection lost");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT] Read error: {ex.Message}");
        }
        
        Console.WriteLine("[CLIENT] Read loop ended");
    }

    public async Task WriteMessagesLoop(CancellationToken token)
    {
        Console.WriteLine("[CLIENT] Write loop started");
        
        try
        {
            while (!token.IsCancellationRequested && _pipeClient?.IsConnected == true)
            {
                if (_sendQueue.TryDequeue(out var message))
                {
                    try
                    {
                        var bytes = Encoding.UTF8.GetBytes(message);
                        
                        lock (_pipeLock)
                        {
                             _pipeClient.WriteAsync(bytes, 0, bytes.Length, token);
                             _pipeClient.FlushAsync();
                        }
                        
                        Console.WriteLine($"[CLIENT] Sent: {message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CLIENT] Write error: {ex.Message}");
                        break;
                    }
                }
                else
                {
                    // Ждем немного, если очередь пуста
                    await Task.Delay(10, token);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Нормальное завершение
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT] Write loop error: {ex.Message}");
        }
        
        Console.WriteLine("[CLIENT] Write loop ended");
    }

}