using System.Collections.ObjectModel;
using System.Windows;
using ConnectInfo;
using ISeeU.Application.Contracts;

namespace Wpf;

public partial class MainWindow : Window, ISurveillanceClient
{
    private readonly List<MonitoredElement> _monitoredElements = new();
    private ElementInfo? _currentElement;

    // Чекбоксы свойств выбранного элемента (источник для PropertiesList).
    private readonly ObservableCollection<PropertyChoice> _propertyChoices = new();

    private readonly ServiceConnection _connection;

    public MainWindow()
    {
        InitializeComponent();
        PropertiesList.ItemsSource = _propertyChoices;
        _connection = new ServiceConnection(this);
        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await _connection.ConnectAsync();
            StatusText.Text = "Connected";
        }
        catch (Exception ex)
        {
            StatusText.Text = "Disconnected";
            AddNotification($"Connection failed: {ex.Message}");
        }
    }

    #region ClickLogic

    private void PickElementBtn_Click(object sender, RoutedEventArgs e)
    {
        var picker = new ElementPickerWindow();

        picker.ElementSelected += async (s, point) =>
        {
            try
            {
                if (_connection.Service == null)
                {
                    AddNotification("Not connected to service");
                    return;
                }

                // Прямой RPC-вызов вместо "request|x,y" + ожидания ответа по префиксу.
                var element = await _connection.Service.FindElementAsync((int)point.X, (int)point.Y);
                _currentElement = element;
                AddNotification($"Selected: {(string.IsNullOrEmpty(element.Name) ? "(no name)" : element.Name)} at ({point.X}, {point.Y})");

                // Узнаём, какие свойства поддерживает элемент. По сети едет только int[],
                // человекочитаемые имена берём локально из каталога.
                var supportedIds = await _connection.Service.GetSupportedPropertiesAsync(element);
                var supported = UiaPropertyCatalog.Resolve(supportedIds);

                _propertyChoices.Clear();
                foreach (var p in supported)
                    _propertyChoices.Add(new PropertyChoice
                    {
                        Id = p.Id,
                        Name = p.Name,
                        DisplayName = p.DisplayName,
                        Kind = p.Kind.ToString(),
                        IsSelected = true
                    });

                AddNotification(supported.Count > 0
                    ? $"Supported: {string.Join(", ", supported.Select(p => p.DisplayName))}"
                    : "No monitorable properties found");

                Activate();
            }
            catch (Exception ex)
            {
                AddNotification($"Find error: {ex.Message}");
            }
        };

        picker.SelectionCancelled += (s, args) =>
        {
            AddNotification("Selection cancelled");
            Activate();
        };

        picker.ShowDialog();
    }

    private async void SubscribeBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_currentElement == null)
        {
            AddNotification("No element selected. Click Pick Element first.");
            return;
        }

        if (_connection.Service == null)
        {
            AddNotification("Not connected to service");
            return;
        }

        // Подписываемся только на ОТМЕЧЕННЫЕ свойства.
        var chosen = _propertyChoices
            .Where(c => c.IsSelected)
            .Select(c => new PropertyInfo { Id = c.Id, Name = c.Name })
            .ToList();

        if (chosen.Count == 0)
        {
            AddNotification("Select at least one property to monitor");
            return;
        }

        var elementToSubscribe = new ElementInfo
        {
            ElementId = _currentElement.ElementId ?? Guid.NewGuid().ToString(),
            Name = _currentElement.Name,
            X = _currentElement.X,
            Y = _currentElement.Y,
            Width = _currentElement.Width,
            Height = _currentElement.Height,
            Properties = chosen
        };

        try
        {
            await _connection.Service.SubscribeAsync(elementToSubscribe);

            var newElement = new MonitoredElement
            {
                ElementInfo = elementToSubscribe,
                SubscribedProperties = new HashSet<int>(chosen.Select(p => p.Id))
            };

            _monitoredElements.Add(newElement);
            ElementsList.Items.Add(newElement);
            UpdateCount();

            AddNotification($"Monitoring {_currentElement.Name} ({string.Join(", ", chosen.Select(p => p.Name))})");
        }
        catch (Exception ex)
        {
            AddNotification($"Subscribe error: {ex.Message}");
        }
    }

    private async void UnsubscribeClick(object sender, RoutedEventArgs e)
    {
        if (ElementsList.SelectedItem is not MonitoredElement selected)
        {
            AddNotification("No element selected");
            return;
        }

        try
        {
            if (selected.ElementInfo != null && _connection.Service != null)
                await _connection.Service.UnsubscribeAsync(selected.ElementInfo);
        }
        catch (Exception ex)
        {
            AddNotification($"Unsubscribe error: {ex.Message}");
        }

        ElementsList.Items.Remove(selected);
        _monitoredElements.Remove(selected);
        UpdateCount();
        AddNotification($"Stopped monitoring: {selected.Name}");
    }

    private async void ClearAllBtn_Click(object sender, RoutedEventArgs e)
    {
        foreach (var element in _monitoredElements)
        {
            if (element.ElementInfo == null || _connection.Service == null) continue;
            try { await _connection.Service.UnsubscribeAsync(element.ElementInfo); }
            catch (Exception ex) { AddNotification($"Unsubscribe error: {ex.Message}"); }
        }

        _monitoredElements.Clear();
        _currentElement = null;
        ElementsList.Items.Clear();
        _propertyChoices.Clear();
        UpdateCount();
        AddNotification("All monitoring stopped");
    }

    private void UpdateCount() => ElementCountText.Text = $"Elements: {_monitoredElements.Count}";

    #endregion

    #region ISurveillanceClient (push from service)

    // Вызывается сервером через RPC. Выполняется НЕ на UI-потоке -> маршалим в Dispatcher.
    public void OnElementPropertyChanged(string elementName, string propertyName, string value)
    {
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
            "IsEnabled" => $"{elementName} стал {displayValue}",
            "IsOffscreen" => $"{elementName} {displayValue}",
            "ToggleState" => $"{elementName} {displayValue}",
            "Value" => $"Значение {elementName}: {value}",
            _ => $"{elementName}: {propertyName} = {value}"
        };

        AddNotification(messageText);
    }

    #endregion

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

    protected override void OnClosed(EventArgs e)
    {
        _connection.Dispose();
        base.OnClosed(e);
    }
}

public class MonitoredElement
{
    public ElementInfo? ElementInfo { get; set; }
    public HashSet<int> SubscribedProperties { get; set; } = new();
    public string Name => ElementInfo?.Name ?? "";
}

// Пункт списка чекбоксов: одно свойство + флаг "следить".
public class PropertyChoice
{
    public int Id { get; set; }
    public string Name { get; set; } = "";        // имя в протоколе ("Value")
    public string DisplayName { get; set; } = "";  // имя для UI
    public string Kind { get; set; } = "";
    public bool IsSelected { get; set; } = true;
    public string KindLabel => $"· {Kind}";
}
