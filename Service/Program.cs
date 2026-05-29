
#region MyRegion



using ISeeU.Application.Contracts;
using ISeeU.Application.Convertor;
using ISeeU.Application.CommandHandlers;
using ISeeU.Application.Services;
using ISeeU.Infrastructure.Serialization;
using ISeeU.Infrastructure.Transport;
using ISeeU.Infrastructure.UIAutomation.WindowsOC;

namespace Service;

public class Program
{
    private static NamedPipeServerService? _pipeServer;
    private static SurveillanceManager? _manager;
    private static IElementConvertor? _elementConvertor;
    private static ClientRequestHandler? _requestHandler;
    private static JsonMessageSerialize? _converter;
    private static CancellationTokenSource? _cts;
    private static MessageParser? _parser;
    private static SubscribeHandler? _subscribeHandler;
    private static UnsubscribeHandler? _unsubscribeHandler;
    private static ClientRequestHandler? _clientRequestHandler;
    private static UIAutomationServiceWindows? _uiAutomationService;

    [STAThread]
    static void Main()
    {
        Console.WriteLine("=== ISeeU Service Started ===");

        // Инициализация
        var provider = new UIAutomationServiceWindows();
        var targetFabric = new TargetFabricWinApi30(provider.GetCUIAutomation());


        _manager = new SurveillanceManager(provider, targetFabric);
        _converter = new JsonMessageSerialize();
        _elementConvertor = new Convertor(provider);
        _pipeServer = new NamedPipeServerService(ConnectInfo.ConnectInfo.PipeName);
        _parser = new MessageParser();  

        //command
        _subscribeHandler = new SubscribeHandler(_manager, null, _elementConvertor, _pipeServer, _converter);
        _unsubscribeHandler = new UnsubscribeHandler(_subscribeHandler, _manager, _converter);
        _requestHandler = new ClientRequestHandler(_manager, _unsubscribeHandler, provider);

        // Запуск фоновой проверки
        _cts = new CancellationTokenSource();
        Task.Run(() => _manager.CheckAllElementIsAlive(_cts.Token));

        // Запуск сервера
        _pipeServer.StartListening(OnMessageReceived);

        Console.WriteLine($"Pipe name: {ConnectInfo.ConnectInfo.PipeName}");
        Console.WriteLine("Waiting for connections...");
        Console.WriteLine("Press Ctrl+C to exit\n");

        // Ожидание завершения
        var waitEvent = new ManualResetEvent(false);
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\nShutting down...");
            _cts?.Cancel();
            _pipeServer?.StopListening();
            waitEvent.Set();
        };

        waitEvent.WaitOne();
    }

    private static string OnMessageReceived(string message)
    {
        Console.WriteLine($"[RECEIVED] {message}");

        var (command, payload) = _parser.Parse(message);
        if (command == "keepalive")
        {
            return "alive|";
        }
        var output = _requestHandler.Handle(command, payload);
        
        Console.WriteLine(output);

        return output;
    }
}



#endregion