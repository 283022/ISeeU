using ConnectInfo;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

// Чистое правило выбора способа наблюдения за свойством.
// Вынесено из фабрики, чтобы решение poll vs event можно было протестировать без COM.
public static class ObserveStrategyResolver
{
    // Неизвестные свойства по умолчанию пробуем через UIA-событие.
    public static ObserveStrategy Resolve(int propertyId) =>
        UiaPropertyCatalog.Get(propertyId)?.Strategy ?? ObserveStrategy.Event;
}
