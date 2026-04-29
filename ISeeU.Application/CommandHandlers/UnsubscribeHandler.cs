using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Services;

namespace ISeeU.Application.CommandHandlers;

public class UnsubscribeHandler(CommandHandler next): CommandHandler(next)
{
    private CommandHandler _next = next;


    protected override bool CanHandle(string command)
    {
        return string.Equals(command, "unsubscribe");
    }

    public override void Handle(string message, SurveillanceManager manager)
    {
        if (CanHandle(message))
        {
            //this is a PLUG
            throw new NotImplementedException();
        }
        else
        {
            _next.Handle(message, manager);
        }
    }

}