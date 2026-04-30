using ConnectInfo;
using ISeeU.Application.AbstractClasses;
using ISeeU.Application.Contracts;
using ISeeU.Application.Services;

namespace ISeeU.Application.CommandHandlers;

public class SubscribeHandler : CommandHandler
{
    private readonly SurveillanceManager _manager;
    private readonly IElementConvertor _mapper;
    private readonly ISignalTransmitter _transmitter; 
    private readonly IMessageConverter _converter;
    
    protected override bool CanHandle(string command)
    {
        return string.Equals(command, "subscribe");
    }

    public SubscribeHandler(
        SurveillanceManager manager,
        CommandHandler next,
        IElementConvertor mapper,
        ISignalTransmitter transmitter,
        IMessageConverter converter) : base(next, manager)
    {
        _converter = converter;
        _manager = manager;
        _mapper = mapper;
        _transmitter = transmitter;
        _next = next;
    }

    public override void Handle(string message, string payload)
    {
        if (!CanHandle(message))
        {
            _next.Handle(message, payload);
            return;
        }

        var info = _converter.Deserialize<ElementInfo>(payload);
        var element = _mapper.ToDomain(info);

        foreach (var propertyId in info.Properties)
        {
            _manager.Add(element,propertyId.Id,
                (propId, newValue) =>
            {
                _transmitter.Send($"Changed: {element.Name} - {newValue}");
            });
        }
    }
}