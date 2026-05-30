using System.Text.Json;
using System.Windows;
using ConnectInfo;

namespace Wpf;

public partial class MainWindow : Window
{
    private List<MonitoredElement> _monitoredElements = new();
    private ElementInfo? _currentElement;
    private Connection _connection;

    // Основные свойства для отслеживания (пользователю нужны только эти)
    private static readonly List<PropertyInfo> ImportantProperties = new()
    {
        new PropertyInfo { Id = 30005, Name = "Name" },
        new PropertyInfo { Id = 30010, Name = "IsEnabled" },
        new PropertyInfo { Id = 30022, Name = "IsOffscreen" },
        new PropertyInfo { Id = 30045, Name = "Value" },      // ValuePattern
        new PropertyInfo { Id = 30041, Name = "ToggleState" } // TogglePattern
    };

    public MainWindow()
    {
        InitializeComponent();
        _connection = new Connection(ProcessMessageHandler);
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _ = _connection.ConnectToServiceAsync();
    }

    #region ClickLogic

    private void PickElementBtn_Click(object sender, RoutedEventArgs e)
    {
        var picker = new ElementPickerWindow();
    
        picker.ElementSelected += (s, point) =>
        {
            var message = $"request|{point.X},{point.Y}";
            _connection.Send(message);
        
            Dispatcher.Invoke(() =>
            {
                AddNotification($"Position selected: ({point.X}, {point.Y})");
                this.Activate();
            });
        };
    
        picker.SelectionCancelled += (s, args) =>
        {
            Dispatcher.Invoke(() =>
            {
                AddNotification("Selection cancelled");
                this.Activate();
            });
        };
    
        picker.ShowDialog();
    }

    private void SubscribeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentElement == null)
        {
            AddNotification("No element selected. Click Pick Element first.");
            return;
        }

        // Подписываемся на все важные свойства сразу
        var elementToSubscribe = new ElementInfo
        {
            ElementId = _currentElement.ElementId ?? Guid.NewGuid().ToString(),
            Name = _currentElement.Name,
            X = _currentElement.X,
            Y = _currentElement.Y,
            Width = _currentElement.Width,
            Height = _currentElement.Height,
            Properties = ImportantProperties  // ← Все важные свойства
        };

        var message = $"subscribe|{JsonSerializer.Serialize(elementToSubscribe)}";
        _connection.Send(message);

        var newElement = new MonitoredElement
        {
            ElementInfo = elementToSubscribe,
            SubscribedProperties = new HashSet<int>(ImportantProperties.Select(p => p.Id))
        };
        
        _monitoredElements.Add(newElement);
        ElementsList.Items.Add(newElement);
        
        AddNotification($"Monitoring {_currentElement.Name} (Name, IsEnabled, Value, ToggleState)");
    }

    private void UnsubscribeClick(object sender, RoutedEventArgs e)
    {
        var selected = ElementsList.SelectedItem as MonitoredElement;
        
        if (selected == null)
        {
            AddNotification("No element selected");
            return;
        }
        
        if (selected.ElementInfo != null)
        {
            var message = $"unsubscribe|{JsonSerializer.Serialize(selected.ElementInfo)}";
            _connection.Send(message);
        }
        
        ElementsList.Items.Remove(selected);
        _monitoredElements.Remove(selected);
        
        AddNotification($"Stopped monitoring: {selected.Name}");
    }

    private void ClearAllBtn_Click(object sender, RoutedEventArgs e)
    {
        foreach (var element in _monitoredElements)
        {
            if (element.ElementInfo == null) continue;
            var message = $"unsubscribe|{JsonSerializer.Serialize(element.ElementInfo)}";
            _connection.Send(message);
        }

        _monitoredElements.Clear();
        _currentElement = null;
        ElementsList.Items.Clear();
        
        AddNotification("All monitoring stopped");
    }

    #endregion

    #region ProcessMessage

    private string[] MessageParser(string message)
    {
        var parts = message.Split('|');
        return parts.Length < 2 ? [] : parts;
    }

    private void ProcessMessageHandler(string message)
    {
        var parts = MessageParser(message);
        if (parts.Length < 2) return;

        switch (parts[0])
        {
            case "connected":
                Dispatcher.Invoke(() => StatusText.Text = "Connected");
                break;

            case "elementinfo":
                try
                {
                    var element = JsonSerializer.Deserialize<ElementInfo>(parts[1]);
                    if (element != null)
                    {
                        _currentElement = element;
                        AddNotification($"Selected: {element.Name}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENT] Parse error: {ex.Message}");
                }
                break;

            case "subscribed":
                AddNotification("Monitoring started");
                break;

            case "unsubscribed":
                AddNotification("Monitoring stopped");
                break;

            case "changed":
                // parts[1] = element name
                // parts[2] = property name (уже читаемое, т.к. сервер присылает имя)
                // parts[3] = new value
                var propertyName = parts[2];
                var value = parts[3];
                
                // Человеко-понятный вывод
                var displayValue = propertyName switch
                {
                    "IsEnabled" => value == "True" ? "доступен" : "недоступен",
                    "IsOffscreen" => value == "True" ? "скрыт" : "видим",
                    "ToggleState" => value switch
                    {
                        "1" => "включен",
                        "0" => "выключен",
                        _ => "неопределен"
                    },
                    _ => value
                };
                
                var messageText = propertyName switch
                {
                    "IsEnabled" => $"{parts[1]} стал {displayValue}",
                    "IsOffscreen" => $"{parts[1]} {displayValue}",
                    "ToggleState" => $"{parts[1]} {displayValue}",
                    "Value" => $"Значение {parts[1]}: {value}",
                    _ => $"{parts[1]}: {propertyName} = {value}"
                };
                
                AddNotification(messageText);
                break;
        }
    }

    private void AddNotification(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
            NotificationsList.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {message}");
            
            while (NotificationsList.Items.Count > 50)
                NotificationsList.Items.RemoveAt(NotificationsList.Items.Count - 1);
        });
    }

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        _connection.OnClosed();
        base.OnClosed(e);
    }
}

public class MonitoredElement
{
    public ElementInfo? ElementInfo { get; set; }
    public HashSet<int> SubscribedProperties { get; set; } = new();
    public string Name => ElementInfo?.Name ?? "";
}

public class PropertyItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public bool IsSubscribed { get; set; }
}