using System.Diagnostics;
using Meilisearch;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Msh.Api.Middleware;
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Logamos o erro com o TraceId para rastreamento fácil
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        logger.LogError(exception, "Erro não tratado detectado. TraceId: {TraceId}. Mensagem: {Message}",
            traceId, exception.Message);

        // 2. Mapeamos a exceção para o padrão ProblemDetails
        var (statusCode, title, detail) = MapException(exception);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        // Adicionamos o TraceId no retorno para que o suporte/dev possa achar no log
        problemDetails.Extensions.Add("traceId", traceId);

        // 3. Configuramos a resposta
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Sucesso: O erro foi tratado pelo nosso pipeline
    }

    private static (int StatusCode, string Title, string Detail) MapException(Exception exception)
    {
        return exception switch
        {
            // Erros disparados pelo SDK do Meilisearch
            MeilisearchApiError meiliError => (
                Convert.ToInt32(meiliError.Code),
                "Erro no Serviço de Busca",
                "O Meilisearch retornou um erro técnico. Verifique se o índice e a chave estão corretos."
            ),

            // Timeout (Quando o Meili demora mais que os 5s que configuramos)
            OperationCanceledException or TaskCanceledException => (
                StatusCodes.Status504GatewayTimeout,
                "Tempo de Resposta Excedido",
                "O serviço de busca demorou muito para responder. Tente novamente em alguns segundos."
            ),

            // Erros de validação (comum em MediatR/FluentValidation)
            ArgumentException or InvalidOperationException => (
                StatusCodes.Status400BadRequest,
                "Requisição Inválida",
                exception.Message
            ),

            //// Erros de validação do FluentValidation (captura as mensagens amigáveis que definimos nos Validators)
            //FluentValidation.ValidationException valEx => (
            //    StatusCodes.Status400BadRequest,
            //    "Erro de Validação",
            //    string.Join(" ", valEx.Errors.Select(e => e.ErrorMessage))
            //),

            // Erro genérico (O famoso 500)
            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro Interno no Servidor",
                "Ocorreu um erro inesperado. Nossa equipe técnica já foi notificada."
            )
        };
    }
}