using ConnectInfo;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Contracts;

public interface IElementConvertor
{
    public IElement ToDomain(ElementInfo dto);
}