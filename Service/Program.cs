using ISeeU.Application.Convertor;
using ISeeU.Application.Services;
using ISeeU.Infrastructure.Transport;
using ISeeU.Infrastructure.UIAutomation.WindowsOC;

namespace Service;

public class Program
{
    [STAThread]
    static async Task Main()
    {
        Console.WriteLine("=== ISeeU Service Started ===");

        // Композиция зависимостей (онион: инфраструктура -> приложение).
        var provider = new UIAutomationServiceWindows();
        var targetFabric = new TargetFabricWinApi30(provider.GetCUIAutomation());
        var manager = new SurveillanceManager(provider, targetFabric);
        var convertor = new Convertor(provider);
        var service = new SurveillanceService(manager, convertor, provider);

        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            Console.WriteLine("\nShutting down...");
            cts.Cancel();
        };

        // Фоновая чистка мёртвых правил.
        _ = Task.Run(() => manager.CheckAllElementIsAlive(cts.Token));

        var server = new JsonRpcPipeServer(ConnectInfo.ConnectInfo.PipeName, service);
        Console.WriteLine($"Pipe name: {ConnectInfo.ConnectInfo.PipeName}");
        Console.WriteLine("Waiting for connections... Press Ctrl+C to exit\n");

        try
        {
            await server.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
        }

        Console.WriteLine("Service stopped.");
    }
}
