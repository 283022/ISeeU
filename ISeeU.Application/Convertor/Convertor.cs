using System.Drawing;
using ConnectInfo;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Convertor;

public class Convertor(IUIAutomationProvider provider) : IElementConvertor
{
    public IElement ToDomain(ElementInfo dto)
    {
        var point = new Point(dto.X, dto.Y);
        return provider.FindElement(point);
    }
}