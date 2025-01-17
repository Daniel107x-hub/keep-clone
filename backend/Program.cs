

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Middlewares;
using backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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

builder.Services.AddMemoryCache();
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("isAdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "ADMIN"));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder.WithOrigins("http://localhost:50000").AllowAnyMethod().AllowAnyHeader().AllowCredentials());
});

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseCors("AllowAll");
    app.UseSwagger(); // Adds middleware to expose openapi document
    app.UseSwaggerUI(); // Add middleware that serves swaggerui
}

app.MapPost("/api/Account", async (UserRegisterDto user, KeepContext _context, PasswordHasher<User> passwordHasher) => {
    User newUser  = new User {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Password = user.Password,
        PhoneNumber = user.PhoneNumber,
        UserName = user.UserName,
        IsActive = true
    };
    var userExists = await _context.Users.Where(u => u.UserName == newUser.UserName).FirstOrDefaultAsync();
    if(userExists != null) {
        return Results.BadRequest("Username already exists");
    }
    newUser.Password = passwordHasher.HashPassword(newUser, newUser.Password);
    try
    {
        _context.Users.Add(newUser);
        _context.SaveChanges();
    }
    catch (DbUpdateException e)
    {
        return Results.BadRequest("An error occurred while creating the account");
    }
    //await SendConfirmationEmail(newUser);
    return Results.Created($"/api/Account/{user.UserName}", user);
});

app.MapGet("/api/Account", (KeepContext _context) =>
{
    IEnumerable<User> users = _context.Users;
    return Results.Ok(users);
}).RequireAuthorization("isAdminPolicy");

app.MapGet("/api/Account/CurrentUser", () => Results.Ok()).RequireAuthorization();

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
}).RequireAuthorization("isAdminPolicy");

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

app.MapPost("/api/Account/Login", async (UserLoginDto user, KeepContext _context, PasswordHasher<User> passwordHasher, IMemoryCache _memoryCache) =>
{
    User foundUser;
    try
    {
        foundUser = await _context.Users.Where(u => u.UserName == user.UserName).FirstAsync();
    }
    catch (InvalidOperationException ex)
    {
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
    var token = GenerateToken(foundUser);
    // var cookieOptions = new CookieOptions
    // {
    //     // HttpOnly = true,
    //     Expires = DateTime.Now.AddMinutes(30),
    //     MaxAge = TimeSpan.FromMinutes(30),
    //     SameSite = SameSiteMode.None,
    //     Secure = true
    // };
    // var cookie = cookieOptions.CreateCookieHeader("token", token);
    // // response.Headers.Add(HeaderNames.SetCookie, cookie.ToString());
    // httpContext.Response.Headers.Append(HeaderNames.SetCookie, cookie.ToString());
    _memoryCache.Set(foundUser.UserName, token, new MemoryCacheEntryOptions
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    });
    return Results.Ok(token);
});

app.MapPost("/api/Account/Logout", async (KeepContext _context, IHttpContextAccessor httpContextAccessor, IMemoryCache cache) => {
    var context = httpContextAccessor.HttpContext;
    var user = context.User;
    User foundUser = await _context.Users.Include(u => u.Notes).Where(u => u.UserName == user.Identity.Name).FirstAsync();
    if(foundUser == null || !foundUser.IsActive) {
        return Results.NotFound();
    }
    cache.Remove(foundUser.UserName);
    return Results.Ok();
}).RequireAuthorization();

app.MapPost("/api/Notes", async (NoteDto note, KeepContext _context, IHttpContextAccessor httpContextAccessor) => {
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
    note.Id = newNote.Id;
    return Results.Created($"/api/Note/{newNote.Id}", note);
}).RequireAuthorization();

app.MapGet("/api/Notes", async ([FromServices]KeepContext _context, IHttpContextAccessor httpContextAccessor) => {
    var context = httpContextAccessor.HttpContext;
    var user = context.User;
    User foundUser = await _context.Users.Include(u => u.Notes).Where(u => u.UserName == user.Identity.Name).FirstAsync();
    if(foundUser == null || !foundUser.IsActive) {
        return Results.NotFound();
    }
    var notesDto = foundUser.Notes.Select(n => new NoteDto {
        Id = n.Id,
        Title = n.Title,
        Content = n.Content
    });
    return Results.Ok(notesDto);
}).RequireAuthorization();

app.MapDelete("/api/Notes/{noteId}",
    async (long noteId, KeepContext _context, IHttpContextAccessor httpContextAccessor) =>
    {   
        var context = httpContextAccessor.HttpContext;
        var user = context.User;
        User foundUser = await _context.Users.Include(u => u.Notes).Where(u => u.UserName == user.Identity.Name).FirstAsync();
        if(foundUser == null || !foundUser.IsActive) {
            return Results.NotFound();
        }
        Note note = foundUser.Notes.Where(n => n.Id == noteId).FirstOrDefault();
        if(note == null) {
            return Results.NotFound();
        }
        foundUser.Notes.Remove(note);
        _context.SaveChanges();
        return Results.NoContent();
    }
).RequireAuthorization();

app.UseAuthentication();
app.UseAuthorization();
app.UseVerifyTokenCache();
app.Run();

async Task SendConfirmationEmail(User user)
{
    // Send confirmation email
    var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
    var client = new SendGridClient(apiKey);
    var templateId = app.Configuration["SendGrid:TemplateId"];
    var encodedUserName = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(user.UserName));
    var msg = MailHelper.CreateSingleTemplateEmail(
        from: new EmailAddress("daniel107x@outlook.es", "Example User"),
        to: new EmailAddress(user.Email, user.FirstName),
        templateId: templateId,
        dynamicTemplateData: new
        {
            userName = user.UserName,
            url = "https://daniel107x-keep.duckdns.org/api/Account/Activate" + encodedUserName
        }
    );
    await client.SendEmailAsync(msg);
}

string GenerateToken(User user)
{
    var role = user.IsAdmin ? "ADMIN" : "USER";
    var claims = new[] {
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Role, role),
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
    return new JwtSecurityTokenHandler().WriteToken(token);
}