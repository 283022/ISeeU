using ISeeU.Application.CommandHandlers;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Entities;

namespace ISeeU.Application.Services;

public class SurveillanceManager(IUIAutomationProvider provider)
{
    private bool _flag = false;
    private readonly List<SurveillanceRule> _surveillanceRules = new();

    public void Subscribe(SurveillanceRule surveillanceRule)
    {
        _surveillanceRules.Add(surveillanceRule);
    }

    public void UnSubscribe(SurveillanceRule surveillanceRule)
    {
        _surveillanceRules.Remove(surveillanceRule);
    }

    public async Task CheckAllElementIsAlive(CancellationToken token)
    {
        if (_flag == true) return;
        while (!token.IsCancellationRequested)
        {
            _flag = true;
            await Task.Delay(1000, token);
            RemoveDeadRules();
        }
        
    }

    private void RemoveDeadRules()
    {
        lock (_surveillanceRules)
        {
            _surveillanceRules.RemoveAll(r => provider.ElementIsAlive(r.Target.Element));
        }
    }
}