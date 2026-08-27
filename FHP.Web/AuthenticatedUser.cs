namespace FHP.Web
{
    /// <summary>
    /// Small, display-only snapshot of the logged-in user, read from the auth cookie's
    /// claims. Deliberately excludes PasswordHash — claims should never carry
    /// sensitive data.
    /// </summary>
    public class AuthenticatedUser
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
