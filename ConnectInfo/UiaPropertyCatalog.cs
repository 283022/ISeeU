namespace ConnectInfo;

// Тип значения свойства — чтобы клиент понимал, как его отрисовать.
public enum UiaValueKind
{
    String,
    Bool,
    Toggle,
    Number,
    Other
}

// Как наблюдать за свойством: событием UIA или поллингом.
public enum ObserveStrategy
{
    Event,
    Polling
}

// Описание одного UIA-свойства. Это СТАТИКА: id свойств UIA фиксированы ОС,
// поэтому имена не нужно гонять по сети — клиент резолвит их локально из каталога.
public sealed record UiaProperty(
    int Id,
    string Name,          // имя в протоколе/логике ("Value", "IsEnabled", ...)
    string DisplayName,   // человекочитаемое имя для UI
    UiaValueKind Kind,
    ObserveStrategy Strategy,
    // Для свойств, стоящих за паттерном (Value, ToggleState), поддержку нельзя узнать
    // через PollForPotentialSupportedProperties — её проверяют по IsXxxPatternAvailable.
    // Здесь храним id этого "availability"-свойства (или null, если свойство обычное).
    int? PatternAvailabilityId = null);

// Единый источник правды о свойствах: id <-> имя, тип значения, стратегия наблюдения.
// Сюда же переехало знание "poll vs event", которое раньше было захардкожено
// в TargetFabricWinApi30, и корректный id ToggleState (30086, а не 30041).
public static class UiaPropertyCatalog
{
    public static IReadOnlyList<UiaProperty> All { get; } = new[]
    {
        new UiaProperty(30005, "Name",        "Name",         UiaValueKind.String, ObserveStrategy.Event),
        new UiaProperty(30010, "IsEnabled",   "Is Enabled",   UiaValueKind.Bool,   ObserveStrategy.Event),
        new UiaProperty(30022, "IsOffscreen", "Is Offscreen", UiaValueKind.Bool,   ObserveStrategy.Event),
        // 30043 = IsValuePatternAvailable, 30041 = IsTogglePatternAvailable
        new UiaProperty(30045, "Value",       "Value",        UiaValueKind.String, ObserveStrategy.Polling, PatternAvailabilityId: 30043),
        new UiaProperty(30086, "ToggleState", "Toggle State", UiaValueKind.Toggle, ObserveStrategy.Polling, PatternAvailabilityId: 30041),
    };

    private static readonly Dictionary<int, UiaProperty> ById = All.ToDictionary(p => p.Id);

    public static bool TryGet(int id, out UiaProperty property) => ById.TryGetValue(id, out property!);

    public static UiaProperty? Get(int id) => ById.TryGetValue(id, out var p) ? p : null;

    public static bool IsKnown(int id) => ById.ContainsKey(id);

    // Резолвим набор id в полноценные описания, отбрасывая неизвестные нам.
    public static IReadOnlyList<UiaProperty> Resolve(IEnumerable<int> ids) =>
        ids.Where(IsKnown).Select(id => ById[id]).ToList();
}
