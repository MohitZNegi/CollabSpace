using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CollabSpace.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // Read from environment variable so the real URL is used
            // when running EF Core commands locally.
            // Set this environment variable in your terminal before
            // running dotnet ef commands.
            var connectionString =
                Environment.GetEnvironmentVariable(
                    "ConnectionStrings__DatabaseConnection")
                ?? "Host=localhost;Port=5432;Database=collabspace_dev;" +
                   "Username=postgres;Password=postgres";

            // Convert Railway URL format if needed
            if (connectionString.StartsWith("postgresql://") ||
                connectionString.StartsWith("postgres://"))
            {
                var uri = new Uri(connectionString);
                var userInfo = uri.UserInfo.Split(':');
                connectionString =
                    $"Host={uri.Host};" +
                    $"Port={uri.Port};" +
                    $"Database={uri.AbsolutePath.TrimStart('/')};" +
                    $"Username={userInfo[0]};" +
                    $"Password={Uri.UnescapeDataString(userInfo[1])};" +
                    $"SSL Mode=Require;" +
                    $"Trust Server Certificate=true;";
            }

            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}