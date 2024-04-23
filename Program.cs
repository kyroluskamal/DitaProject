using System.Text.Json.Serialization;
using AppConfgDocumentation.Data;
using AppConfgDocumentation.Models;
using AppConfgDocumentation.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.FileProviders;
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
        // Type = SecuritySchemeType.ApiKey
    });
    options.DocumentFilter<IgnoreMethodsFilter>();

    options.OperationFilter<SecurityRequirementsOperationFilter>();

});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .ConfigureWarnings(warnings =>
            {
                warnings.Throw(SqlServerEventId.SavepointsDisabledBecauseOfMARS);
            });
    options.EnableDetailedErrors();
    options.EnableSensitiveDataLogging();
});

builder.Services.AddScoped<IDitaFileCreationService, DitaFileCreationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
builder.Services.AddAuthorization();
builder.Services.AddIdentityApiEndpoints<ApplicationUser>(op =>
{
    op.User.RequireUniqueEmail = true;
    op.Password.RequiredLength = 8;
    op.Password.RequireNonAlphanumeric = true;
    op.Password.RequireUppercase = false;
    op.Password.RequireLowercase = false;
    op.Password.RequireDigit = true;
    op.SignIn.RequireConfirmedEmail = false;
})
    .AddRoles<IdentityRole<int>>()
    .AddUserManager<UserManager<ApplicationUser>>()
    .AddRoleManager<RoleManager<IdentityRole<int>>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddSingleton<IDitaValidationService, DitaValidationService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
});
builder.Services.AddScoped<IDbContextInitializer, DbContextInitializer>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var dbContextInitializer = scope.ServiceProvider.GetRequiredService<IDbContextInitializer>();
    dbContextInitializer.Initialize().GetAwaiter().GetResult();
}
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
app.UseAuthentication();
app.UseAuthorization();
app.MapPost("/users", async (AppRegisterRequest request, UserManager<ApplicationUser> userManager, HttpContext context) =>
{
    var roleManager = context.RequestServices.GetRequiredService<RoleManager<IdentityRole>>();

    var user = new ApplicationUser
    {
        UserName = request.Email,
        Email = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, request.Password);

    if (result.Succeeded)
    {
        var roleExists = await roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
            await roleManager.CreateAsync(new IdentityRole(request.Role));

        await userManager.AddToRoleAsync(user, request.Role);

        return Results.Ok(new { message = "User created successfully", data = user });
    }
    return Results.BadRequest(new { message = "User creation failed", errors = result.Errors });
});

app.MapGet("/users", async (UserManager<ApplicationUser> userManager) =>
{
    var users = await userManager.Users.ToListAsync();
    return Results.Ok(new { message = "Users retrieved successfully", data = users });
});

// app.MapGet("/users/{id}", async (int id, UserManager<ApplicationUser> userManager) =>
// {
//     var user = await userManager.FindByIdAsync(id.ToString());
//     if (user == null)
//         return Results.NotFound(new { message = "User not found" });
//     return Results.Ok(new { message = "User retrieved successfully", data = user });
// });

app.MapPut("/users/{id}", async (int id, AppRegisterRequest request, UserManager<ApplicationUser> userManager,
RoleManager<IdentityRole<int>> roleManager) =>
{
    var user = await userManager.FindByIdAsync(id.ToString());
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    user.FirstName = request.FirstName;
    user.LastName = request.LastName;
    user.Email = request.Email;
    user.UserName = request.Email;

    var result = await userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        // var role = await userManager.GetRolesAsync(user);
        if (!await userManager.IsInRoleAsync(user, request.Role))
        {
            var roleExit = await roleManager.RoleExistsAsync(request.Role);
            if (!roleExit)
                await roleManager.CreateAsync(new IdentityRole<int>(request.Role));
            await userManager.AddToRoleAsync(user, request.Role);
        }



        return Results.Ok(new { message = "User updated successfully", data = user });
    }

    return Results.BadRequest(new { message = "User update failed", errors = result.Errors });

});

//getUser by email
app.MapGet("/users/{email}", async (string email, UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });
    return Results.Ok(new { message = "User retrieved successfully", data = user });
});

//create new role
app.MapPost("/roles", async (string roleName, RoleManager<IdentityRole<int>> roleManager) =>
{
    var role = new IdentityRole<int>(roleName);
    var result = await roleManager.CreateAsync(role);
    if (result.Succeeded)
        return Results.Ok(new { message = "Role created successfully" });
    return Results.BadRequest(new { message = "Role creation failed", errors = result.Errors });
});

//get all roles
app.MapGet("/roles", async (RoleManager<IdentityRole<int>> roleManager) =>
{
    return Results.Ok(await roleManager.Roles.ToListAsync());
});
//get Role by Id

//get role by userId
// app.MapGet("/roles/{userId}", async (int userId, UserManager<ApplicationUser> userManager) =>
// {
//     var user = await userManager.FindByIdAsync(userId.ToString());
//     if (user == null)
//         return Results.NotFound(new { message = "User not found" });
//     var roles = await userManager.GetRolesAsync(user);
//     return Results.Ok(new { message = "Roles retrieved successfully", data = roles });
// });

//get role by userEmail
app.MapGet("/roles/{email}", async (string email, UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });
    var roles = await userManager.GetRolesAsync(user);
    return Results.Ok(new { message = "Roles retrieved successfully", data = roles });
});
//sign in
app.MapGet("/signin", async (string email, string password, UserManager<ApplicationUser> userManager) =>
{
    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
        return Results.NotFound(new { message = "User not found" });

    var result = await userManager.CheckPasswordAsync(user, password);
    if (result)
        return Results.Ok(new { message = "User signed in successfully", data = user });

    return Results.BadRequest(new { message = "User sign in failed" });
});


app.MapControllers();
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
}); app.UseCors("AllowAll");
app.Run();
