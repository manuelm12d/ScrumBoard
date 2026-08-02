using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
using ScrumBoard.Application.Abstractions.Persistence;
using ScrumBoard.Application.Abstractions.Reporting;
using ScrumBoard.Application.Abstractions.Security;
using ScrumBoard.Application.Abstractions.Time;
using ScrumBoard.Infrastructure.Persistence;
using ScrumBoard.Infrastructure.Persistence.Queries;
using ScrumBoard.Infrastructure.Persistence.Repositories;
using ScrumBoard.Infrastructure.Reporting;
using ScrumBoard.Infrastructure.Security;
using ScrumBoard.Infrastructure.Time;

namespace ScrumBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionString)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<PasswordHashingOptions>(configuration.GetSection(PasswordHashingOptions.SectionName));

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IBoardColumnRepository, BoardColumnRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IBoardQuery, BoardQuery>();
        services.AddScoped<IProjectReportQuery, ProjectReportQuery>();

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // Every exporter is registered under the same port; ReportService picks one by key, so a third
        // format only adds a line here and never touches the existing classes.
        services.AddSingleton<IReportExporter, PdfProjectReportExporter>();
        services.AddSingleton<IReportExporter, ExcelProjectReportExporter>();

        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}
