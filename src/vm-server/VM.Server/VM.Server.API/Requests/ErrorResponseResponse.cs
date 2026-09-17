namespace VM.Server.API.Dtos;

public sealed record ErrorResponseDto(string Code, string Message, IReadOnlyDictionary<string, object>? Details);
