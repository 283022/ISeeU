using ConnectInfo;
using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Contracts;
using ISeeU.Application.Services;

namespace ISeeU.Application.CommandHandlers;

public class SubscribeHandler(
    SurveillanceManager manager,
    CommandHandler? next,
    IElementConvertor mapper,
    ICommunicationChannel communicationChannel,
    IMessageConverter converter)
    : CommandHandler(next, manager)
{
    private readonly SurveillanceManager _manager = manager;

    protected override bool CanHandle(string command)
    {
        return string.Equals(command, "subscribe");
    }

    public override string Handle(string message, string payload)
    {
        
        if (!CanHandle(message))
        {
            if (_next == null)
            {
                return "error| ";
            }
            Console.WriteLine("22");
            var messageReturn = _next.Handle(message, payload);
            return messageReturn;
        }

        try
        {
            var info = converter.Deserialize<ElementInfo>(payload);
            if (info == null)
                return "subscribe|error|invalid payload";
            
            var element = mapper.ToDomain(info);

            Console.WriteLine(element);
            
            foreach (var prop in info.Properties)
            {
                Console.WriteLine(prop.Name);
                _manager.Add(element, prop.Id, (id, value) =>
                {
                    Console.WriteLine("ВСЕ ИЗМЕНИЛОСЬ");
                    // Отправляем уведомление об изменении через канал связи
                    var notification = $"changed|{info.Name}|{id}|{value}";
                    communicationChannel.Send(notification);
                });
            }

            return $"subscribed|{info.Name}-ok";
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return $"subscribe|error-{ex.Message}";
        }
    }
}