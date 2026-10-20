using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Practicals.Services.Day3
{
    public sealed class LabDbHealth(IConfiguration config) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
        {
            try
            {
                var cs = config.GetConnectionString("Day2Db");
                if (string.IsNullOrWhiteSpace(cs)) 
                    return HealthCheckResult.Unhealthy("No connection string");
                await using var conn = new SqlConnection(cs);
                await conn.OpenAsync(ct);
                return HealthCheckResult.Healthy("SQL Server reachable");
            }
            catch { return HealthCheckResult.Unhealthy("SQL Server unavailable"); }
        }
    }

}
