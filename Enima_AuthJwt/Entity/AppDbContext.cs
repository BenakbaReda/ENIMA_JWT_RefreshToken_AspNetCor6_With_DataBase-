using Microsoft.EntityFrameworkCore;

namespace Enima_AuthJwt
{
    public class AppDbContext : DbContext
    {
   

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
 
        }

 

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }

        #region OnModelCreating 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>()
                           .HasData(
                               new User
                               {
                                   uuid = Guid.NewGuid(),
                                   Email = "admin",
                                   Password = "admin"
                               },
                               new User
                               {
                                   uuid = Guid.NewGuid(),
                                   Email = "user",
                                   Password = "user"
                               }
                           );

  
        }
        #endregion
       
    }
}