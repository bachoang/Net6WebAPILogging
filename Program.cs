using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using System.Diagnostics;
using Serilog;


// https://github.com/datalust/dotnet6-serilog-example

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("starting up");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console()
        .ReadFrom.Configuration(ctx.Configuration));

    // Add services to the container.
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

    // Enable PII for logging
    IdentityModelEventSource.ShowPII = true;
    // Configure middleware events
    builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidAudiences = new List<string> { "api://a56797c4-e6f7-4d8c-89cc-0b3cc64d8b3e", "a56797c4-e6f7-4d8c-89cc-0b3cc64d8b3e" },
            ValidIssuers = new List<string> { "https://sts.windows.net/<tenant ID>/", "https://login.microsoftonline.com/<tenant ID>/v2.0" }
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = ctx =>
            {
                string message = "[OnTokenValidated]: ";
                message += $"token: {ctx.SecurityToken.ToString()}";
                Log.Information(message); 
                return Task.CompletedTask;
            },
            OnMessageReceived = ctx =>
            {
                string message = "[OnMessageReceived]: ";
                ctx.Request.Headers.TryGetValue("Authorization", out var BearerToken);
                if (BearerToken.Count == 0)
                    BearerToken = "no Bearer token sent\n";
                message += "Authorization Header sent: " + BearerToken + "\n";
                Log.Information(message);
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = ctx =>
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                string message = $"[OnAuthenticationFailed]: {ctx.Exception.ToString()}";
                Log.Error(message);
                // Debug.WriteLine("[OnAuthenticationFailed]: Authentication failed with the following error: ");
                // Debug.WriteLine(ctx.Exception);
                return Task.CompletedTask;
            },
            OnChallenge = ctx =>
            {
                // Debug.WriteLine("[OnChallenge]: I can do stuff here! ");
                Log.Information("[OnChallenge]");
                return Task.CompletedTask;
            },
            OnForbidden = ctx =>
            {
                Log.Information("[OnForbidden]");
                return Task.CompletedTask;
            }
        };
    });

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    // builder.Services.AddEndpointsApiExplorer();
    // builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        // app.UseSwagger();
        // app.UseSwaggerUI();
        // do something
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}



