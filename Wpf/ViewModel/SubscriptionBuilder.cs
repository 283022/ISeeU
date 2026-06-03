using ConnectInfo;

namespace Wpf;

// Чистая логика подготовки подписки: какие свойства выбраны и из чего собрать
// ElementInfo для отправки сервису. Вынесено из MainWindow ради тестируемости.
public static class SubscriptionBuilder
{
    // Поддерживаемое свойство каталога -> пункт чекбокса (по умолчанию отмечен).
    public static PropertyChoice ToChoice(UiaProperty property) => new()
    {
        Id = property.Id,
        Name = property.Name,
        DisplayName = property.DisplayName,
        Kind = property.Kind.ToString(),
        IsSelected = true
    };

    // Только отмеченные пользователем свойства.
    public static List<PropertyInfo> SelectedProperties(IEnumerable<PropertyChoice> choices) =>
        choices
            .Where(c => c.IsSelected)
            .Select(c => new PropertyInfo { Id = c.Id, Name = c.Name })
            .ToList();

    // Собираем DTO для подписки на основе выбранного элемента и отмеченных свойств.
    public static ElementInfo Build(ElementInfo current, List<PropertyInfo> chosen) => new()
    {
        ElementId = current.ElementId ?? Guid.NewGuid().ToString(),
        Name = current.Name,
        X = current.X,
        Y = current.Y,
        Width = current.Width,
        Height = current.Height,
        Properties = chosen
    };
}
