using ConnectInfo;
using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Contracts;
using ISeeU.Application.Services;

namespace ISeeU.Application.CommandHandlers;

public class UnsubscribeHandler(CommandHandler next, SurveillanceManager manager, IMessageConverter converter): CommandHandler(next, manager)
{
    private CommandHandler _next = next;
    private readonly IMessageConverter _converter = converter;
    private readonly SurveillanceManager _manager = manager;

    protected override bool CanHandle(string command)
    {
        return string.Equals(command, "unsubscribe");
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
        var info = _converter.Deserialize<ElementInfo>(payload);
        _manager.UnSubscribe(info);
        return "unsubscribed|" + info.Name;
    }

}