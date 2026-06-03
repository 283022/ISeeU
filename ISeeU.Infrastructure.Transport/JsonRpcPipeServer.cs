using System.Diagnostics.CodeAnalysis;
using System.IO.Pipes;
using ISeeU.Application.Contracts;
using ISeeU.Application.Services;
using StreamJsonRpc;

namespace ISeeU.Infrastructure.Transport;

// Транспорт: named pipe + StreamJsonRpc.
// На каждое подключение поднимаем JsonRpc поверх дуплексного pipe-стрима:
//  - регистрируем сервис как локальную цель (его методы зовёт клиент);
//  - получаем прокси клиента (через него сервис шлёт push).
// ВАЖНО: pipe здесь БАЙТОВЫЙ (PipeTransmissionMode.Byte) — границы сообщений
// режет сам StreamJsonRpc (Content-Length заголовки), message-mode больше не нужен.
[ExcludeFromCodeCoverage]
public class JsonRpcPipeServer(string pipeName, SurveillanceService service)
{
    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            try
            {
                Console.WriteLine("[Pipe] Waiting for connection...");
                await pipe.WaitForConnectionAsync(token);
                Console.WriteLine("[Pipe] Client connected!");

                var formatter = new SystemTextJsonFormatter();
                var handler = new HeaderDelimitedMessageHandler(pipe, pipe, formatter);

                using var rpc = new JsonRpc(handler);
                rpc.AddLocalRpcTarget(service);          // методы сервиса доступны клиенту
                service.Client = rpc.Attach<ISurveillanceClient>(); // прокси для push
                rpc.StartListening();

                // Ждём, пока клиент не отключится (или отмена).
                await rpc.Completion;
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Обрыв соединения StreamJsonRpc сообщает исключением — это норма.
                Console.WriteLine($"[Pipe] Connection closed: {ex.Message}");
            }
            finally
            {
                service.Client = null;
                await pipe.DisposeAsync();
            }
        }
    }
}
