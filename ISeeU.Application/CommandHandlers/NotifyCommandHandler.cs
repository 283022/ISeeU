using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Services;

namespace ISeeU.Application.CommandHandlers;

public class NotifyCommandHandler(CommandHandler next) : CommandHandler(next)
{
    protected override bool CanHandle(string command)
    {
        throw new NotImplementedException();
    }

    public override void Handle(string message, SurveillanceManager manager)
    {
        throw new NotImplementedException();
    }
}