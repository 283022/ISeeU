using ConnectInfo;

namespace ISeeU.Application.Contracts;

// Контракт того, что умеет сервис. Клиент (WPF) получает прокси этого интерфейса
// через StreamJsonRpc и вызывает методы как локальные.
// Методы возвращают Task -> уходят как JSON-RPC request (ждём ответ).
public interface ISurveillanceService
{
    Task<ElementInfo> FindElementAsync(int x, int y);

    // Возвращает ТОЛЬКО id поддерживаемых (и известных нам) свойств — компактно.
    // Имена клиент резолвит сам через UiaPropertyCatalog, текст по сети не едет.
    Task<int[]> GetSupportedPropertiesAsync(ElementInfo element);

    Task SubscribeAsync(ElementInfo element);
    Task UnsubscribeAsync(ElementInfo element);
}
