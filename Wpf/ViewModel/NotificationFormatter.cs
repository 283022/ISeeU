namespace Wpf;

// Чистое форматирование push-уведомления от сервиса в текст для UI.
// Вынесено из MainWindow.OnElementPropertyChanged, чтобы покрыть юнит-тестами
// (раньше эта логика жила внутри окна и зависела от Dispatcher/WPF).
public static class NotificationFormatter
{
    public static string Format(string elementName, string propertyName, string value)
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

        return propertyName switch
        {
            "IsEnabled" => $"{elementName} стал {displayValue}",
            "IsOffscreen" => $"{elementName} {displayValue}",
            "ToggleState" => $"{elementName} {displayValue}",
            "Value" => $"Значение {elementName}: {value}",
            _ => $"{elementName}: {propertyName} = {value}"
        };
    }
}
