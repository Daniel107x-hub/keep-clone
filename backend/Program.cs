

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using SendGrid;
using SendGrid.Helpers.Mail;

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
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
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

app.MapPost("/api/Account", async (UserRegisterDto user, KeepContext _context, PasswordHasher<User> passwordHasher) => {
    User newUser  = new User {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Password = user.Password,
        PhoneNumber = user.PhoneNumber,
        UserName = user.UserName,
        IsActive = false
    };
    newUser.Password = passwordHasher.HashPassword(newUser, newUser.Password);
    _context.Users.Add(newUser);
    _context.SaveChanges();
    // Send confirmation email
    var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    var client = new SendGridClient(apiKey);
    var templateId = app.Configuration["SendGrid:TemplateId"];
    var encodedUserName = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(newUser.UserName));
    var msg = MailHelper.CreateSingleTemplateEmail(
        from: new EmailAddress("daniel107x@outlook.es", "Example User"),
        to: new EmailAddress(newUser.Email, newUser.FirstName),
        templateId: templateId,
        dynamicTemplateData: new
        {
            userName = user.UserName,
            url = "https://daniel107x-keep.duckdns.org/api/Account/Activate" + encodedUserName
        }
    );
    await client.SendEmailAsync(msg);
    return Results.Created($"/api/Account/{user.UserName}", user);
});

app.MapGet("/api/Account", (KeepContext _context) =>
{
    IEnumerable<User> users = _context.Users;
    return Results.Ok(users);
});

app.MapDelete("/api/Account/{userName}", async (string userName, KeepContext _context) =>
{
    User user;
    try
    {
        user = await _context.Users.Where(u => u.UserName == userName).FirstAsync();
    }
    catch (InvalidOperationException e)
    {
        return Results.NotFound();
    }
    _context.Users.Remove(user);
    _context.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/api/Account/Activate/{encodedUserName}", async (string encodedUserName, KeepContext _context) =>
{
    var userName = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedUserName));
    User user;
    try
    {
        user = await _context.Users.Where(u => u.UserName == userName).FirstAsync();
    }
    catch (InvalidOperationException ex)
    {
        return Results.NotFound();
    }
    if(user.IsActive) return Results.Ok(user);
    user.IsActive = true;
    _context.SaveChanges();
    return Results.Ok("The user has been activated successfully!");
});

app.MapPost("/api/Account/Login", async (UserLoginDto user, KeepContext _context, PasswordHasher<User> passwordHasher) =>
{
    var foundUser = await _context.Users.Where(u => u.UserName == user.UserName).FirstAsync();
    if (foundUser == null) {
        return Results.NotFound();
    }

    if (!foundUser.IsActive)
    {
        return Results.BadRequest("The user is not active!");
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

app.MapPost("/api/Note", async (NoteDto note, KeepContext _context, IHttpContextAccessor httpContextAccessor) => {
    var context = httpContextAccessor.HttpContext;
    var user = context.User;
    var foundUser = await _context.Users.Include(u=> u.Notes).Where(u => u.UserName == user.Identity.Name).FirstAsync();
    if(foundUser == null || !foundUser.IsActive) {
        return Results.NotFound();
    }
    Note newNote = new Note {
        Title = note.Title,
        Content = note.Content,
        User = foundUser
    };
    foundUser.Notes.Add(newNote);
    _context.SaveChanges();
    return Results.Created($"/api/Note/{newNote.Id}", note);
}).RequireAuthorization();

app.MapGet("/api/Note", async ([FromServices]KeepContext _context, IHttpContextAccessor httpContextAccessor) => {
    var context = httpContextAccessor.HttpContext;
    var user = context.User;
    User foundUser = await _context.Users.Include(u => u.Notes).Where(u => u.UserName == user.Identity.Name).FirstAsync();
    if(foundUser == null || !foundUser.IsActive) {
        return Results.NotFound();
    }
    var notesDto = foundUser.Notes.Select(n => new NoteDto {
        Title = n.Title,
        Content = n.Content
    });
    return Results.Ok(notesDto);
}).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.Run();
