using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using RealTimeChatApp.Infrastructure.Persistence;
using RealTimeChatApp.Domain.Entities;


public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {

            // Remove the app's ApplicationDbContext registration (PostgreSQL)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add InMemory DB
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbForTesting");
            });

            // Remove existing authentication schemes
                        services.RemoveAll(typeof(Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider));
                        services.RemoveAll(typeof(Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>));

            // Add your test auth scheme (using TestAuthHandler)
                        services.AddAuthentication("Test")
                            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                        // Add Authorization policy if you want
                        services.AddAuthorization(options =>
                        {
                            options.AddPolicy("Authenticated", policy => policy.RequireAuthenticatedUser());
                        });
            // Ensure DB is created
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
                        {
                            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                            context.Database.EnsureCreated();

                            var testGroupId = Guid.Parse("fea4427e-015b-462b-ace4-0d8ec2e42fb5");
                            if (!context.Groups.Any(g => g.Id == testGroupId))
                            {
                                context.Groups.Add(new Group
                                {
                                    Id = testGroupId,
                                    Name = "Test Group"
                                });
                                context.SaveChanges();
                            }
                        }

        });
         builder.Configure(app =>
         {
             app.UseRouting(); // Must be present

             app.UseAuthentication();
             app.UseAuthorization();

             app.UseEndpoints(endpoints =>
             {
                 endpoints.MapControllers();
                 // Map hubs if you have any
             });
         });

    }
}
