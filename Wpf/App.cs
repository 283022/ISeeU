using System.Windows;
using Wpf;
using Application = System.Windows.Application;

public class App : Application
{
    readonly MainWindow mainWindow;
 
    public App(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;
        this.MainWindow = mainWindow;  // ← КЛЮЧЕВОЕ: устанавливаем главное окно
        this.ShutdownMode = ShutdownMode.OnMainWindowClose;  // ← Явно указываем
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        mainWindow.Show();
        base.OnStartup(e);
    }
}