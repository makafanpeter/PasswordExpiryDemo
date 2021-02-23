using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using PasswordPoliciesDemo.API.Infrastructure.Persistence;

namespace PasswordPoliciesDemo.API.Infrastructure
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly DemoContext _context;
        public DatabaseHealthCheck(DemoContext context)
        {
            _context = context;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default(CancellationToken))
        {

            try
            {


                if (await _context.Database.CanConnectAsync(cancellationToken))
                {
                    return await Task.FromResult(
                        HealthCheckResult.Healthy("Database connection is ok."));
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(status: context.Registration.FailureStatus, exception: ex);
            }

            return await Task.FromResult(HealthCheckResult.Unhealthy("Database connection is not ok."));
        }


    }
}
