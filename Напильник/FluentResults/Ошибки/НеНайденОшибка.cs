namespace FluentResults.Ошибки;

/// <summary>
///     Аргумент не должен быть null
/// </summary>
public class НеНайденОшибка : Error {
    public НеНайденОшибка() : this("") { }

    public НеНайденОшибка(string message) : base(message) { }

    public НеНайденОшибка(string message, Error causedBy) : base(message, causedBy) { }
}