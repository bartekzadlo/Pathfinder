using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Pathfinder.Modules.Routing.Domain;

namespace Pathfinder.Extensions;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception, "An unhandled exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server Error",
            Detail = "An unexpected error occurred processing your request."
        };

        // Handle specific Domain exceptions
        if (exception is CapacityExceededException domEx)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Domain Rule Violation";
            problemDetails.Detail = domEx.Message;
        }
        else if (exception is ArgumentException argEx)
        {
            problemDetails.Status = StatusCodes.Status400BadRequest;
            problemDetails.Title = "Invalid Argument";
            problemDetails.Detail = argEx.Message;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // We handled the exception
    }
}
