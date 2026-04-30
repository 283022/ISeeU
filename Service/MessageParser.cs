namespace Service;

public class MessageParser
{
    public (string Command, string Payload) Parse(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return (string.Empty, string.Empty);

        var parts = message.Split('|', 2);
        var command = parts[0].Trim().ToLowerInvariant();
        var payload = parts.Length > 1 ? parts[1] : string.Empty;

        return (command, payload);
    }
}