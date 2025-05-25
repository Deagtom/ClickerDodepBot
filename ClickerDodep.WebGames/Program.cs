using ClickerDodep.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<UserRepository>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    string connString = config.GetConnectionString("Postgres")!;
    return new UserRepository(connString);
});

builder.Services.AddControllers();

var app = builder.Build();



app.UseStaticFiles();
app.MapControllers();

app.MapGet("/roulette", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/roulette/index.html");
});

app.MapGet("/flappy", async context =>
{
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync("wwwroot/flappycoin/index.html");
});

app.Run();