using System.Text.Json;
using ISeeU.Application.Contracts;

namespace ISeeU.Infrastructure.Serialization;

public class JsonMessageSerialize : IMessageConverter
{
    public T Deserialize<T>(string payload) => JsonSerializer.Deserialize<T>(payload);
}