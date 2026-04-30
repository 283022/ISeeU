using System.Drawing;
using ConnectInfo;
using ISeeU.Application.CommandHandlers;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Entities;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Services;

public class SurveillanceManager(IUIAutomationProvider provider, ITargetFabric targetFabric)
{
    private bool _flag = false;
    private readonly List<SurveillanceRule> _surveillanceRules = new();
    private readonly ITargetFabric _targetFabric = targetFabric;

    public void Add(IElement element, int propertyId, Action<int, object> callback)
    {
        var targetInfo = new TargetInfo(element);
        var observer = _targetFabric.CreateTargetObserver(propertyId, callback);

        var rule = new SurveillanceRule(
            propertyId,
            targetInfo,
            callback,
            observer
        );
        
        Subscribe(rule);
    }

    private void Subscribe(SurveillanceRule surveillanceRule)
    {
        _surveillanceRules.Add(surveillanceRule);
    }

    public void UnSubscribe(ElementInfo elementInfo)
    {
        
        var delete = provider.FindElement(new Point(elementInfo.X, elementInfo.Y));
        var toRemove = _surveillanceRules.Where(rule => rule.Target._element.ProcessId != delete.ProcessId).ToList();
        foreach (var rule in toRemove)
            _surveillanceRules.Remove(rule);
        
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
        _flag = false;
    }

    private void RemoveDeadRules()
    {
        lock (_surveillanceRules)
        {
            _surveillanceRules.RemoveAll(r => provider.ElementIsAlive(r.Target.Element));
        }
    }
}