using ConnectInfo;

namespace ISeeU.Application.Contracts;

// Контракт того, что умеет сервис. Клиент (WPF) получает прокси этого интерфейса

public interface ISurveillanceService
{
    Task<ElementInfo> FindElementAsync(int x, int y);

    // Возвращает ТОЛЬКО id поддерживаемых (и известных нам) свойств
    // Имена клиент резолвит сам через UiaPropertyCatalog, текст по сети не едет.
    Task<int[]> GetSupportedPropertiesAsync(ElementInfo element);

    Task SubscribeAsync(ElementInfo element);
    Task UnsubscribeAsync(ElementInfo element);
}
