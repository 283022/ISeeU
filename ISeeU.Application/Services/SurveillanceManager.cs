using System.Drawing;
using ConnectInfo;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Entities;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Services;

public class SurveillanceManager(IUIAutomationProvider provider, ITargetFabric targetFabric)
{
    private bool _flag = false;
    private readonly object _sync = new();
    private readonly List<SurveillanceRule> _surveillanceRules = new();
    private readonly ITargetFabric _targetFabric = targetFabric;

    public void Add(IElement element, int propertyId, Action<int, object> callback)
    {
        var targetInfo = new TargetInfo(element);
        var observer = _targetFabric.CreateTargetObserver(element,propertyId, callback);

        var rule = new SurveillanceRule(
            propertyId,
            targetInfo,
            callback,
            observer
        );
        
        
        observer.Start();
        Subscribe(rule);
    }

    private void Subscribe(SurveillanceRule surveillanceRule)
    {
        lock (_sync)
            _surveillanceRules.Add(surveillanceRule);
    }

    public void UnSubscribe(ElementInfo elementInfo)
    {
        var target = provider.FindElement(new Point(elementInfo.X, elementInfo.Y));

        lock (_sync)
        {
            // Раньше здесь было "!=", из-за чего удалялись ВСЕ правила, кроме нужного.
            var toRemove = _surveillanceRules
                .Where(rule => IsSameElement(rule.Target.Element, target))
                .ToList();

            foreach (var rule in toRemove)
            {
                rule.Observer.Stop(); // обязательно гасим наблюдатель, иначе утечка
                _surveillanceRules.Remove(rule);
            }
        }
    }

    private static bool IsSameElement(IElement a, IElement b)
    {
        return a.ProcessId == b.ProcessId
               && a.Name == b.Name
               && a.BoundingRectangle.X == b.BoundingRectangle.X
               && a.BoundingRectangle.Y == b.BoundingRectangle.Y;
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
        lock (_sync)
        {
            var dead = _surveillanceRules
                .Where(r => !provider.ElementIsAlive(r.Target.Element))
                .ToList();

            foreach (var rule in dead)
            {
                rule.Observer.Stop();
                _surveillanceRules.Remove(rule);
            }
        }
    }
}