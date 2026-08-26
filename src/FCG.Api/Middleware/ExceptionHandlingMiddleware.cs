using FCG.Application.Common;
using FCG.Domain.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString();
            await HandleExceptionAsync(context, ex, correlationId);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception, string? correlationId)
    {
        var (status, title) = exception switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Requisição inválida."),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "Não autorizado."),
            ConflictException => (StatusCodes.Status409Conflict, "Conflito de dados."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado."),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno.")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro não tratado. CorrelationId: {CorrelationId}", correlationId);
        }
        else
        {
            logger.LogWarning(exception, "Erro de negócio. CorrelationId: {CorrelationId}", correlationId);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = exception.Message,
            Extensions = { ["correlationId"] = correlationId }
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(problem);
    }
}
