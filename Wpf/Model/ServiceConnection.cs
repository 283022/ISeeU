using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using ISeeU.Application.Contracts;
using StreamJsonRpc;

namespace Wpf;

// Тонкий клиент поверх StreamJsonRpc. Заменяет старый Connection с ручными
// read/write-петлями, очередью и парсингом "command|payload".
// Транспорт (named pipe + RPC) — интеграционная часть, юнит-тестами не покрывается.
[ExcludeFromCodeCoverage]
public class ServiceConnection(ISurveillanceClient callback) : IDisposable
{
    private NamedPipeClientStream? _pipe;
    private JsonRpc? _rpc;

    // Прокси сервиса: вызовы методов уходят на сервер как RPC-запросы.
    public ISurveillanceService? Service { get; private set; }

    public async Task ConnectAsync()
    {
        _pipe = new NamedPipeClientStream(
            ".",
            ConnectInfo.ConnectInfo.PipeName,
            PipeDirection.InOut,
            PipeOptions.Asynchronous);

        await _pipe.ConnectAsync(5000);

        var formatter = new SystemTextJsonFormatter();
        var handler = new HeaderDelimitedMessageHandler(_pipe, _pipe, formatter);

        _rpc = new JsonRpc(handler);
        _rpc.AddLocalRpcTarget(callback);            // сервер может звать наш OnElementPropertyChanged
        Service = _rpc.Attach<ISurveillanceService>(); // а мы — методы сервера
        _rpc.StartListening();
    }

    public void Dispose()
    {
        _rpc?.Dispose();
        _pipe?.Dispose();
    }
}
