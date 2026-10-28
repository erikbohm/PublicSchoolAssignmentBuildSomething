var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient("ServiceB", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ServiceB:BaseUrl"]
        ?? "http://localhost:5178");

    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

//app.UseAuthorization();

app.MapControllers();

app.Run();
