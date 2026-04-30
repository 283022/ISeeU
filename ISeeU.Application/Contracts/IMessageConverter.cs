namespace ISeeU.Application.Contracts;

public interface IMessageConverter
{
    T Deserialize<T>(string payload);
}
