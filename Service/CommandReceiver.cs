
using ISeeU.Application.CommandHandlers;

namespace Service;

public class CommandReceiver
{
    private readonly SubscribeHandler _subscribeHandler;
    private readonly UnsubscribeHandler _unsubscribeHandler;
    private readonly NotifyHandler _notifyHandler;

    public CommandReceiver(
        SubscribeHandler subscribeHandler,
        UnsubscribeHandler unsubscribeHandler,
        NotifyHandler notifyHandler)
    {
        _subscribeHandler = subscribeHandler;
        _unsubscribeHandler = unsubscribeHandler;
        _notifyHandler = notifyHandler;
    }

    public void Receive(string message)
    {
        // Парсим сообщение (пока просто строка из двух частей)
        var parts = message.Split('|');
        var command = parts[0];
        var payload = parts.Length > 1 ? parts[1] : "";

        switch (command)
        {
            case "subscribe":
                _subscribeHandler.Handle(payload);
                break;
            case "unsubscribe":
                _unsubscribeHandler.Handle(payload);
                break;
            case "notify":
                _notifyHandler.Handle(payload);
                break;
            default:
                Console.WriteLine($"Неизвестная команда: {command}");
                break;
        }
    }
}