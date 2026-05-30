using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.Json;
using ConnectInfo;
using ISeeU.Application.Contracts;
using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Services;
namespace ISeeU.Application.CommandHandlers;

public class ClientRequestHandler(
    SurveillanceManager manager,
    CommandHandler? next,
    IUIAutomationProvider provider)
    : CommandHandler(next, manager)
{
    protected override bool CanHandle(string message)
    {
        return string.Equals(message, "request", StringComparison.OrdinalIgnoreCase);
    }

    public override string Handle(string message, string payload)
    {
        
        if (!CanHandle(message))
        {
            if (_next == null)
            {
                return "error| ";
            }
            
            var messageReturn = _next.Handle(message, payload);
            return messageReturn;
        }
        
        
        try
        {
            // Парсим координаты из payload
            var coords = payload.Split(',');
            if (coords.Length != 2)
                return "request|error|invalid coordinates";
            
            var x = int.Parse(coords[0]);
            var y = int.Parse(coords[1]);
            var point = new Point(x, y);
            
            // Находим элемент по координатам
            var element = provider.FindElement(point);
            
            // Возвращаем DTO с информацией об элементе
            var elementInfo = new ElementInfo
            {
                ElementId = Guid.NewGuid().ToString(),
                Name = element.Name,
                X = element.BoundingRectangle.X,
                Y = element.BoundingRectangle.Y,
                Width = element.BoundingRectangle.Width,
                Height = element.BoundingRectangle.Height,
            };
            
            return $"elementinfo|{JsonSerializer.Serialize(elementInfo)}";
        }
        catch (Exception ex)
        {
            return $"error|{ex.Message}";
        }
    }
}