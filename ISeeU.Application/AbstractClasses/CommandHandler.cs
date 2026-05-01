
using ISeeU.Application.Services;

namespace ISeeU.Application.AbstractClasses;

public abstract class CommandHandler(CommandHandler next, SurveillanceManager manager)
{
    protected CommandHandler _next = next;
    protected abstract bool CanHandle(string command);
    public abstract void Handle(string message, string payload);
}