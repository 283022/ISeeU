using System.Drawing;

namespace ISeeU.Application.Contracts;

public interface IRequestHandler<T>
{
    bool CanHandle(string message);
    T Handle(Point  point);
}