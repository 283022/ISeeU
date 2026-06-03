using Wpf;

namespace ISeeU.Tests;

public class NotificationFormatterTests
{
    public static IEnumerable<object[]> Cases => new[]
    {
        new object[] { "Btn", "IsEnabled",   "True",    "Btn стал доступен" },
        new object[] { "Btn", "IsEnabled",   "False",   "Btn стал недоступен" },
        new object[] { "Win", "IsOffscreen", "True",    "Win скрыт" },
        new object[] { "Win", "IsOffscreen", "False",   "Win видим" },
        new object[] { "Chk", "ToggleState", "1",       "Chk включен" },
        new object[] { "Chk", "ToggleState", "0",       "Chk выключен" },
        new object[] { "Chk", "ToggleState", "2",       "Chk неопределен" },
        new object[] { "Txt", "Value",       "hello",   "Значение Txt: hello" },
        new object[] { "El",  "Name",        "newName", "El: Name = newName" }, // default-ветка
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Format_ProducesHumanReadableText(string element, string property, string value, string expected)
    {
        Assert.Equal(expected, NotificationFormatter.Format(element, property, value));
    }
}
