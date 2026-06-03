namespace ISeeU.Application.Contracts;

// Контракт того, что сервис может вызвать У КЛИЕНТА (push).
// Метод возвращает void -> уходит как JSON-RPC notification (без id, без ответа).
// Это замена старому "changed|name|prop|value".
public interface ISurveillanceClient
{
    void OnElementPropertyChanged(string elementName, string propertyName, string value);
}
