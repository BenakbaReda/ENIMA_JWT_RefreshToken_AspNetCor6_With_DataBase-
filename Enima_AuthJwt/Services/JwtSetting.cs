using System.Text;

namespace Enima_AuthJwt
{
    public static class JwtSetting
    {
        public static readonly string tokenKey = "This is my test private key";
        public static byte[] key = Encoding.ASCII.GetBytes(JwtSetting.tokenKey);

        public static void SeedData(AppDbContext context)
        {
            var User1 = new User
            {
                uuid = Guid.NewGuid(),
                Email = "admin",
                Password = "admin"
            };


            context.Users.Add(User1);

            var User2 = new User
            {
                uuid = Guid.NewGuid(),
                Email = "user",
                Password = "user"
            };

            context.Users.Add(User1);

            context.SaveChanges();
        }
    }


   

}