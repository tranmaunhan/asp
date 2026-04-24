namespace busline_project.Dtos
{
    public record AuthResponse(int UserId, string Username, string FullName, string Email, string Token);
}
