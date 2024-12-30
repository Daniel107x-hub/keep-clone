

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer(); // Add endpoint discovery features

builder.Services.AddSwaggerGen(opts => { // Add swagger configuration for JWT authentication
    opts.SwaggerDoc("v1", new OpenApiInfo{
        Title = "Keep API",
        Version = "v1"
    });
    var security = new OpenApiSecurityScheme {
        Name = HeaderNames.Authorization,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
        Reference = new OpenApiReference {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    opts.AddSecurityDefinition(security.Reference.Id, security);
    opts.AddSecurityRequirement(new OpenApiSecurityRequirement{
        {security, Array.Empty<string>()}
    });
}); // Add swashbuckle services

builder.Services.AddDbContext<KeepContext>();
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSwagger(); // Adds middleware to expose openapi document
app.UseSwaggerUI(); // Add middleware that serves swaggerui

app.MapPost("/api/Account/Register", (User user, IUserRepository users, PasswordHasher<User> passwordHasher) => {
    user.Password = passwordHasher.HashPassword(user, user.Password);
    users.CreateUser(user);
    return Results.Created($"/api/Account/{user.Id}", user);
});

app.MapPost("/api/Account/Login", async (User user, IUserRepository users, PasswordHasher<User> passwordHasher) => {
    var foundUser = await users.GetUserByUserName(user.UserName);
    if (foundUser == null) {
        return Results.NotFound();
    }
    var result = passwordHasher.VerifyHashedPassword(foundUser, foundUser.Password, user.Password);
    if (result == PasswordVerificationResult.Failed) {
        return Results.NotFound();
    }
    var claims = new[] {
        new Claim(ClaimTypes.Name, foundUser.UserName),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(app.Configuration["Jwt:Key"]));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(
        issuer: app.Configuration["Jwt:Issuer"],
        audience: app.Configuration["Jwt:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: creds
    );
    return Results.Ok(new {
        token = new JwtSecurityTokenHandler().WriteToken(token)
    });
});

app.MapPost("/api/Account/Logout", () => {
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/Note", (Note note, INoteRepository notes) => {
    notes.Create(note);
    return Results.Created($"/api/Note/{note.Id}", note);
}).RequireAuthorization();

app.MapGet("/api/Note", (INoteRepository notes) => {
    return Results.Ok(notes.GetAll());
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
