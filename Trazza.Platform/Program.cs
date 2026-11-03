using Trazza.Platform.Billing.Application.Acl;
using Trazza.Platform.Billing.Application.CommandServices;
using Trazza.Platform.Billing.Application.Internal.CommandServices;
using Trazza.Platform.Billing.Application.Internal.OutboundServices;
using Trazza.Platform.Billing.Application.Internal.QueryServices;
using Trazza.Platform.Billing.Application.QueryServices;
using Trazza.Platform.Billing.Domain.Repositories;
using Trazza.Platform.Billing.Infrastructure.Payments.Simulated.Services;
using Trazza.Platform.Billing.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Billing.Interfaces.Acl;
using Trazza.Platform.Execution.Application.Acl;
using Trazza.Platform.Execution.Application.CommandServices;
using Trazza.Platform.Execution.Application.Internal.CommandServices;
using Trazza.Platform.Execution.Application.Internal.QueryServices;
using Trazza.Platform.Execution.Application.QueryServices;
using Trazza.Platform.Execution.Domain.Repositories;
using Trazza.Platform.Execution.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Execution.Interfaces.Acl;
using Trazza.Platform.Iam.Application.Acl;
using Trazza.Platform.Iam.Application.CommandServices;
using Trazza.Platform.Iam.Application.Internal.CommandServices;
using Trazza.Platform.Iam.Application.Internal.OutboundServices;
using Trazza.Platform.Iam.Application.Internal.QueryServices;
using Trazza.Platform.Iam.Application.QueryServices;
using Trazza.Platform.Iam.Domain.Repositories;
using Trazza.Platform.Iam.Infrastructure.Hashing.BCrypt.Services;
using Trazza.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Iam.Infrastructure.Pipeline.Middleware.Extensions;
using Trazza.Platform.Iam.Infrastructure.Tokens.Jwt.Configuration;
using Trazza.Platform.Iam.Infrastructure.Tokens.Jwt.Services;
using Trazza.Platform.Iam.Interfaces.Acl;
using Trazza.Platform.Matchmaking.Application.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.CommandServices;
using Trazza.Platform.Matchmaking.Application.Internal.OutboundServices;
using Trazza.Platform.Matchmaking.Application.Internal.QueryServices;
using Trazza.Platform.Matchmaking.Application.QueryServices;
using Trazza.Platform.Matchmaking.Domain.Repositories;
using Trazza.Platform.Matchmaking.Domain.Services;
using Trazza.Platform.Matchmaking.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Profiles.Application.Acl;
using Trazza.Platform.Profiles.Application.CommandServices;
using Trazza.Platform.Profiles.Application.Internal.CommandServices;
using Trazza.Platform.Profiles.Application.Internal.QueryServices;
using Trazza.Platform.Profiles.Application.QueryServices;
using Trazza.Platform.Profiles.Domain.Repositories;
using Trazza.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Profiles.Interfaces.Acl;
using Trazza.Platform.Reputation.Application.CommandServices;
using Trazza.Platform.Reputation.Application.Internal.CommandServices;
using Trazza.Platform.Reputation.Application.Internal.QueryServices;
using Trazza.Platform.Reputation.Application.QueryServices;
using Trazza.Platform.Reputation.Domain.Repositories;
using Trazza.Platform.Reputation.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Shared.Domain.Repositories;
using Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Interfaces.AspNetCore.OpenApi;
using Trazza.Platform.Shared.Infrastructure.Mediator.Cortex.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Trazza.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Trazza.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;
using Trazza.Platform.Shared.Resources;
using Trazza.Platform.Shared.Resources.Errors;
using Cortex.Mediator.Commands;
using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Scalar.AspNetCore;
using ProblemDetailsFactory = Trazza.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(connectionStringTemplate))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException("Database connection string is not set in the configuration.");

    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .AddInterceptors(serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});

builder.Services.AddLocalization();

builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<CommonMessages>, StringLocalizer<CommonMessages>>();

builder.Services.AddSingleton<ProblemDetailsFactory>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<IamBearerSecurityOperationTransformer>();
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<DispatchDomainEventsInterceptor>();

builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();
builder.Services.AddScoped<IIamContextFacade, IamContextFacade>();

builder.Services.AddScoped<ICarrierProfileRepository, CarrierProfileRepository>();
builder.Services.AddScoped<IMerchantProfileRepository, MerchantProfileRepository>();
builder.Services.AddScoped<ICarrierProfileCommandService, CarrierProfileCommandService>();
builder.Services.AddScoped<ICarrierProfileQueryService, CarrierProfileQueryService>();
builder.Services.AddScoped<IMerchantProfileCommandService, MerchantProfileCommandService>();
builder.Services.AddScoped<IMerchantProfileQueryService, MerchantProfileQueryService>();
builder.Services.AddScoped<IProfilesContextFacade, ProfilesContextFacade>();

builder.Services.AddScoped<IReturnRouteRepository, ReturnRouteRepository>();
builder.Services.AddScoped<IFreightRequestRepository, FreightRequestRepository>();
builder.Services.AddScoped<IMatchProposalRepository, MatchProposalRepository>();
builder.Services.AddScoped<IRouteMatchingService, RouteMatchingService>();
builder.Services.AddScoped<PublicationPolicy>();
builder.Services.AddScoped<IReturnRouteCommandService, ReturnRouteCommandService>();
builder.Services.AddScoped<IReturnRouteQueryService, ReturnRouteQueryService>();
builder.Services.AddScoped<IFreightRequestCommandService, FreightRequestCommandService>();
builder.Services.AddScoped<IFreightRequestQueryService, FreightRequestQueryService>();
builder.Services.AddScoped<IMatchProposalCommandService, MatchProposalCommandService>();
builder.Services.AddScoped<IMatchProposalQueryService, MatchProposalQueryService>();

builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<IShipmentCommandService, ShipmentCommandService>();
builder.Services.AddScoped<IShipmentQueryService, ShipmentQueryService>();
builder.Services.AddScoped<IExecutionContextFacade, ExecutionContextFacade>();

builder.Services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
builder.Services.AddScoped<IReceiptRepository, ReceiptRepository>();
builder.Services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
builder.Services.AddScoped<IPaymentTransactionCommandService, PaymentTransactionCommandService>();
builder.Services.AddScoped<IPaymentTransactionQueryService, PaymentTransactionQueryService>();
builder.Services.AddScoped<IReceiptCommandService, ReceiptCommandService>();
builder.Services.AddScoped<IReceiptQueryService, ReceiptQueryService>();
builder.Services.AddScoped<IBillingContextFacade, BillingContextFacade>();

builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRatingCommandService, RatingCommandService>();
builder.Services.AddScoped<IRatingQueryService, RatingQueryService>();

builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));

builder.Services.AddCortexMediator(
    [typeof(Program)]);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.UseExceptionHandler();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors("AllowAllPolicy");

app.UseRequestAuthorization();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
