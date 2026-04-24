namespace busline_project.Dtos
{
    public record RegisterRequest(string Username, string Password, string FullName, string Email, string? Phone);
    // Identifier can be email or phone (or username). Password is required.
    public record LoginRequest(string Identifier, string Password);
}
