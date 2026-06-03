using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

// Прямая работа с COM-объектом CUIAutomation8 — юнит-тестами не покрывается
// (нужно живое дерево UIA). Проверяется e2e-прогоном на Windows.
[ExcludeFromCodeCoverage]
public class UIAutomationServiceWindows : IUIAutomationProvider
{
    private readonly CUIAutomation8 _automation = new();
    
    public IElement FindElement(Point location)
    {
        var rect = new tagPOINT { x = location.X, y = location.Y };
        var element = _automation.ElementFromPoint(rect);
        return new WindowsElement(element, _automation);
    }

    public IElement GetFocusedElement()
    {
        var element = _automation.GetFocusedElement();
        return new WindowsElement(element, _automation);
    }
    
    public Dictionary<int, string> GetSupportedProperties(IElement element)
    {
        // WindowsElement не реализует IUIAutomationElement, а оборачивает его,
        // поэтому каст к IUIAutomationElement раньше падал всегда.
        if (element is not WindowsElement winElement)
            throw new NotSupportedException("Unsupported element type");

        var uiElement = winElement.GetNativeElement();
        _automation.PollForPotentialSupportedProperties(uiElement, out int[] propertyIds,
            out string[] supportedProperties);
    
        var result = new Dictionary<int, string>();
        for (var i = 0; i < propertyIds.Length; i++)
        {
            var cleanName = supportedProperties[i];
            result[propertyIds[i]] = cleanName;
        }
        return result;
    }

    public object GetCurrentPropertyValue(IElement element, int propertyId)
    {
        if (element is not WindowsElement winElement)
            throw new NotSupportedException("Unsupported element type");

        return winElement.GetNativeElement().GetCurrentPropertyValue(propertyId);
    }

    public bool ElementIsAlive(IElement element)
    {
        if (element is not WindowsElement winElement) throw new NotSupportedException("Unsupported element type");

        // Старый вариант проверял CurrentNativeWindowHandle != 0, но у большинства
        // контролов (кнопки, текст, чекбоксы) нативного окна нет -> handle == 0,
        // и все правила считались "мёртвыми" уже через секунду.
        // Корректно: обратиться к свойству; у мёртвого элемента COM кинет ElementNotAvailable.
        try
        {
            _ = winElement.GetNativeElement().CurrentProcessId;
            return true;
        }
        catch (COMException)
        {
            return false;
        }
    }


    public CUIAutomation8 GetCUIAutomation()
    {
        return _automation;
    }
}