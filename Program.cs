using YmyPixels.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Data class as a Transient service
// Data class will be responsible for database operations
builder.Services.AddTransient<Data>();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) { }
app.UseHttpsRedirection();
// TODO: Configure authentication via Discord (jwt)
app.UseAuthorization();
app.MapControllers();
app.Run();