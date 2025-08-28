// Add more robust error handling to your Program.cs
using ABCRetailers.Services;
using System.Globalization;

namespace ABCRetailers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register Azure Storage Service
            builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();

            // Add Logging with better configuration
            builder.Services.AddLogging(logging =>
            {
                logging.AddConsole();
                logging.AddDebug();
                logging.SetMinimumLevel(LogLevel.Debug);
            });

            var app = builder.Build();

            // Set culture for decimal handling (FIXES PRICE ISSUE)
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            else
            {
                // Detailed error pages in development
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Global error handling middleware
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Unhandled exception occurred");

                    if (!context.Response.HasStarted)
                    {
                        context.Response.Redirect("/Home/Error");
                    }
                }
            });

            app.Run();
        }
    }
}