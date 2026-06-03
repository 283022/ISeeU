namespace ISeeU.Application.Contracts;

// Контракт того, что сервис может вызвать У КЛИЕНТА .
public interface ISurveillanceClient
{
    void OnElementPropertyChanged(string elementName, string propertyName, string value);
}
