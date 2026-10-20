using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Practicals.Services.Day2
{
    public class Day2Errors(ILogger<Day2Errors> log, IHostEnvironment env) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (httpContext.Response.HasStarted) return false;
            var status = 500; var title = "Unexpected server error.";
            if (exception is RuleException r) { status = r.Status; title = r.Message; }
            else if (exception is DbUpdateException { InnerException: SqlException sql })
            {
                if (sql.Number is 2601 or 2627) { status = 409; title = "A unique value already exists."; }
                else if (sql.Number == 547) { status = 409; title = "A related record prevents the change."; }
            }
            if (status >= 500) log.LogError(exception, "Failure {TraceId} on {Path}", httpContext.TraceIdentifier, httpContext.Request.Path);
            else log.LogWarning("Rejected {Status} {TraceId}: {Title}", status, httpContext.TraceIdentifier, title);
            var p = new ProblemDetails
            {
                Status = status,
                Title = title,
                Instance = httpContext.Request.Path,
                Detail = status >= 500 ? (env.IsDevelopment() ? exception.Message : "Contact trainer with traceId.") : null
            };
            p.Extensions["traceId"] = httpContext.TraceIdentifier; httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(p, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
            return true;

        }
    }
}
