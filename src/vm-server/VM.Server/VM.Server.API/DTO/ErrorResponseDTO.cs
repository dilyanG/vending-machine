namespace VM.Server.API.DTO;

public sealed record ErrorResponseDTO(string Code, string Message, IReadOnlyDictionary<string, object>? Details);
