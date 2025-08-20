
using InfoPointServer.Services;

namespace InfoPointServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5000); // HTTP
                options.ListenLocalhost(7051, listenOptions =>
                {
                    listenOptions.UseHttps();
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddSingleton<IProductService, DummyProductService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                    context.Response.Redirect("/swagger");
                else
                    await next();
            });



            app.MapControllers();

            app.Run();
        }
    }
}
