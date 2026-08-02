using System.Net;
using Microsoft.AspNetCore.Mvc;
using ScrumBoard.Application.Common;
using ScrumBoard.Application.Reports;
using ScrumBoard.Domain.Common;

namespace ScrumBoard.Api.Middleware;

/// <summary>
/// Single place where domain and application failures become HTTP status codes. Keeping the mapping
/// here is what lets the inner layers throw meaningful exceptions without referencing ASP.NET.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var (status, title) = Map(exception);

            if (status == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, "Error no controlado en {Path}", context.Request.Path);
            }
            else
            {
                _logger.LogInformation("Solicitud rechazada en {Path}: {Message}", context.Request.Path, exception.Message);
            }

            var problem = new ProblemDetails
            {
                Status = (int)status,
                Title = title,
                // The generic message hides internals on 500 while keeping business errors actionable.
                Detail = status == HttpStatusCode.InternalServerError
                    ? "Ocurrió un error inesperado."
                    : exception.Message,
                Instance = context.Request.Path
            };

            context.Response.Clear();
            context.Response.StatusCode = problem.Status.Value;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }

    private static (HttpStatusCode Status, string Title) Map(Exception exception) => exception switch
    {
        InvalidCredentialsException => (HttpStatusCode.Unauthorized, "Credenciales inválidas"),
        NotFoundException => (HttpStatusCode.NotFound, "Recurso no encontrado"),
        ColumnNotEmptyException => (HttpStatusCode.Conflict, "La columna contiene tareas"),
        ConflictException => (HttpStatusCode.Conflict, "Conflicto con el estado actual"),
        DomainValidationException => (HttpStatusCode.BadRequest, "Datos inválidos"),
        UnsupportedReportFormatException => (HttpStatusCode.BadRequest, "Formato de reporte no soportado"),
        _ => (HttpStatusCode.InternalServerError, "Error interno")
    };
}
