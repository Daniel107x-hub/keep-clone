using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer(); // Add endpoint discovery features
builder.Services.AddSwaggerGen(); // Add swashbuckle services
builder.Services.AddDbContext<KeepContext>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = "/api/Account/AccessDenied";
    });

var app = builder.Build();
app.UseSwagger(); // Adds middleware to expose openapi document
app.UseSwaggerUI(); // Add middleware that serves swaggerui

app.MapGet("/api/Account/AccessDenied", () => {
    return Results.Forbid();
});

app.MapPost("/api/Note", (Note note, INoteRepository notes) => {
    notes.Create(note);
    return Results.Created($"/api/Note/{note.Id}", note);
});

app.MapGet("/api/Note", (INoteRepository notes) => {
    return Results.Ok(notes.GetAll());
});

app.UseAuthentication();
app.UseAuthorization();
app.Run();
