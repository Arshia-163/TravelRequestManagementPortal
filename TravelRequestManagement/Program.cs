using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.Data;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Services.Implementations;
using TravelManagement.Services.Interfaces;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Data.Repositories.Implementation;
using TravelManagement.Services.Shared.Constants;
using TravelRequestManagement.Components;
using TravelRequestManagement;
using TravelRequestManagement.Client.Services;
using TravelRequestManagement.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false; 
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/auth/logout";
    options.AccessDeniedPath = "/login";

    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddControllers();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppRoles.TravelAdminPolicy, p => p.RequireRole(AppRoles.TravelAdmin));
    options.AddPolicy(AppRoles.ManagerPolicy, p => p.RequireRole(AppRoles.Manager));
    options.AddPolicy(AppRoles.DepartmentHeadPolicy, p => p.RequireRole(AppRoles.DepartmentHead));
    options.AddPolicy(AppRoles.CanCreateRequestPolicy, p => p.RequireRole(RoleNames.CanCreateTravelRequestRoles));
});


builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ITravelRequestRepository, TravelRequestRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepository, BookingRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IExtensionRepository, ExtensionRepository>();

builder.Services.AddScoped<IdentitySeederService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ITravelRequestService, TravelRequestService>();
builder.Services.AddScoped<ITravelApprovalService, TravelApprovalService>();
builder.Services.AddScoped<ITripExtensionService, TripExtensionService>();
builder.Services.AddScoped<ICancellationService, CancellationService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAdminService, AdminService>();


builder.Services.AddScoped<ITravelApi, ServerTravelApi>();
builder.Services.AddScoped<AuthState>();
builder.Services.AddScoped<DashboardNotifier>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()

    .AddAuthenticationStateSerialization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

    try
    {
        var seeder = scope.ServiceProvider.GetRequiredService<IdentitySeederService>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        startupLogger.LogError(
            ex,
            "Database migration/seeding failed at startup. The application will still start, but data access will not work until this is resolved. Check the DefaultConnection connection string and that SQL Server is reachable.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseWebAssemblyDebugging();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.Use(async (context, next) =>
{
    if (!context.Request.Path.StartsWithSegments("/api"))
    {
        await next();
        return;
    }

    try
    {
        await next();
    }
    catch (InvalidOperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new { message = ex.Message });
    }
    catch (UnauthorizedAccessException ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new { message = string.IsNullOrWhiteSpace(ex.Message) ? "You are not allowed to do this." : ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new { message = string.IsNullOrWhiteSpace(ex.Message) ? "The requested item was not found." : ex.Message });
    }
    catch (Exception ex)
    {
        
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalApiExceptionHandler");
        logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new { message = "Something went wrong while processing your request. Please try again, and contact support if the problem continues." });
        }
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TravelRequestManagement.Client._Imports).Assembly);

app.MapControllers();

app.Run();
