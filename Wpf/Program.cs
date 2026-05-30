using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Wpf;

public class Program
{
    [STAThread]
    public static void Main()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<MainWindow>();
            })
            .Build();
        
        var mainWindow = host.Services.GetRequiredService<MainWindow>();
        var app = new App(mainWindow);
        app.Run();  
    }
}