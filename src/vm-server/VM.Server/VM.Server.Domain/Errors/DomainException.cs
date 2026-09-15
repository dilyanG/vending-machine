namespace VM.Server.Domain.Errors;

public sealed class DomainException(string code, string message, IReadOnlyDictionary<string, object>? details = null)
    : Exception(message)
{
    public string Code { get; } = code;

    public IReadOnlyDictionary<string, object>? Details { get; } = details;
}
