using System.Text.Json.Serialization;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(x =>
{
    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.OperationFilter<SecurityRequirementsOperationFilter>();

});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IDitaFileCreationService, DitaFileCreationService>();
builder.Services.AddSingleton<IDitaValidationService, DitaValidationService>();

builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(op =>
{
    op.User.RequireUniqueEmail = true;
    op.Password.RequiredLength = 8;
    op.Password.RequireNonAlphanumeric = false;
    op.Password.RequireUppercase = false;
    op.Password.RequireLowercase = false;
    op.Password.RequireDigit = false;
    op.SignIn.RequireConfirmedEmail = false;
    // op.MapType<RegisterRequest>(op => op.Properties(p => p.Email, p => p.Password, p => p.FirstName, p => p.LastName));
})
    .AddRoles<IdentityRole<int>>().AddRoleManager<RoleManager<IdentityRole<int>>>()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddRoleManager<RoleManager<IdentityRole<int>>>()
    .AddPasswordValidator<PasswordValidator<ApplicationUser>>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
}).AddBearerToken(op => { op.BearerTokenExpiration = TimeSpan.FromDays(1); });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}
app.MapIdentityApi<ApplicationUser>();
app.UseHttpsRedirection();
app.MapPost("/user", async (AppRegisterRequest request, UserManager<ApplicationUser> userManager, HttpContext context) =>
{
    var roleManager = context.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName
    };

    var result = await userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
        // Check if the role exists, create if it doesn't
        var roleExists = await roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(request.Role));
        }

        // Add the user to the role
        await userManager.AddToRoleAsync(user, request.Role);

        return Results.Ok("");
    }
    else
    {
        // Handle failure, return the errors
        return Results.BadRequest(result.Errors);
    }
});

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.UseStaticFiles();
app.Run();
