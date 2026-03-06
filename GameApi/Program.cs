using GameApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register the in-memory grid service as a singleton so all requests share the same state.
builder.Services.AddSingleton<GridService>();

// Add MVC controllers.
builder.Services.AddControllers();

// Add OpenAPI / Swagger for easy browser testing.
builder.Services.AddOpenApi();

// Allow the MonoGame client (or any local origin during development) to call the API.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
