using System.Drawing;
using ConnectInfo;
using ISeeU.Application.Contracts;

namespace ISeeU.Application.Services;

// Реализация сервисного контракта. Это "точка входа" для всех вызовов от клиента.
// Раньше эту роль играла цепочка CommandHandler'ов поверх строкового протокола.
public class SurveillanceService(
    SurveillanceManager manager,
    IElementConvertor convertor,
    IUIAutomationProvider provider) : ISurveillanceService
{
    // Прокси клиента, через который шлём push-уведомления.
    // Устанавливается транспортом на каждое подключение (и сбрасывается при дисконнекте).
    public ISurveillanceClient? Client { get; set; }

    public Task<ElementInfo> FindElementAsync(int x, int y)
    {
        var element = provider.FindElement(new Point(x, y));

        // X/Y = исходная кликнутая точка (а не угол bounding-rect),
        // чтобы Subscribe нашёл ровно тот же элемент.
        var info = new ElementInfo
        {
            ElementId = Guid.NewGuid().ToString(),
            Name = element.Name,
            X = x,
            Y = y,
            Width = element.BoundingRectangle.Width,
            Height = element.BoundingRectangle.Height,
        };

        return Task.FromResult(info);
    }

    public Task<int[]> GetSupportedPropertiesAsync(ElementInfo info)
    {
        var element = convertor.ToDomain(info);

        // Обычные свойства видны через potential-set, а паттернные (Value/ToggleState) —
        // нет, поэтому их проверяем отдельно по IsXxxPatternAvailable.
        var potential = provider.GetSupportedProperties(element).Keys.ToHashSet();
        var result = new List<int>();

        foreach (var prop in UiaPropertyCatalog.All)
        {
            bool supported = prop.PatternAvailabilityId is { } availId
                ? IsTrue(provider.GetCurrentPropertyValue(element, availId))
                : potential.Contains(prop.Id);

            if (supported)
                result.Add(prop.Id);
        }

        return Task.FromResult(result.ToArray());
    }

    private static bool IsTrue(object? value) =>
        value is bool b ? b : value != null && Convert.ToBoolean(value);

    public Task SubscribeAsync(ElementInfo info)
    {
        if (info?.Properties == null)
            return Task.CompletedTask;

        var element = convertor.ToDomain(info);
        var elementName = info.Name;

        foreach (var prop in info.Properties)
        {
            var propertyName = prop.Name;
            manager.Add(element, prop.Id, (_, value) =>
            {
                // Push клиенту через прокси. Если клиента нет — Client == null, просто молчим.
                Client?.OnElementPropertyChanged(elementName, propertyName, value?.ToString() ?? "");
            });
        }

        return Task.CompletedTask;
    }

    public Task UnsubscribeAsync(ElementInfo info)
    {
        if (info != null)
            manager.UnSubscribe(info);

        return Task.CompletedTask;
    }
}
