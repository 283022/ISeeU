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

    public override void Handle(string message, string payload)
    {
        if (!CanHandle(message))
        {
            _next.Handle(message, payload);
            return;
        }
        var info = _converter.Deserialize<ElementInfo>(payload);
        _manager.UnSubscribe(info);
    }

}