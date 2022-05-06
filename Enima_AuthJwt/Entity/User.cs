namespace Enima_AuthJwt
{
    public class User : BaseEntity
    {
        public User()
        {
            RefreshTokens = new HashSet<RefreshToken>();
        }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public UserRole? Role { get; set; } = UserRole.User;
 

        public ICollection<RefreshToken> RefreshTokens { get; set; }

    }

}