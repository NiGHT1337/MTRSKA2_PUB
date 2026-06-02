namespace Matroschka2.Security;

internal sealed record AuthorizationResult(bool IsAuthorized, string Message);
