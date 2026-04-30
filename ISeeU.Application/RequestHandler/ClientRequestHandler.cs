using ISeeU.Application.Contracts;
using System.Drawing;
using ConnectInfo;

namespace ISeeU.Application.CommandHandlers;

public class ClientRequestHandler : IRequestHandler<ElementInfo>
{
    private readonly IUIAutomationProvider _provider;

    public ClientRequestHandler(IUIAutomationProvider provider)
    {
        _provider = provider;
    }

    public bool CanHandle(string message)
    {
        return Equals(message, "request");
    }

    public ElementInfo Handle(Point coordinates)
    {
        // Находим элемент по координатам
        var element = _provider.FindElement(coordinates);

        // Возвращаем DTO с информацией об элементе
        return new ElementInfo
        {
            Name = element.Name,
            X = element.BoundingRectangle.X,
            Y = element.BoundingRectangle.Y,
            Width = element.BoundingRectangle.Width,
            Height = element.BoundingRectangle.Height,
            Properties = element.GetSupportedProperties()
                .Select(id => new PropertyInfo { Id = id, Name = id.ToString() })
                .ToList()
        };
    }
}