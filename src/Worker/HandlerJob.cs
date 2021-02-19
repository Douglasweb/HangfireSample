using Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Worker
{
    public class HandlerJob : IHandlerJob
    {
        private readonly ILogger<HandlerJob> _logger;

        public HandlerJob(ILogger<HandlerJob> logger)
        {
            _logger = logger;
        }

        public async Task HandleSync(string jobId, CancellationToken ct)
        {
            try
            {
                _logger.LogInformation($"Starting of {nameof(HandleSync)} with {jobId}");

                await Task.Delay(TimeSpan.FromSeconds(30), ct);

                _logger.LogInformation($"Finishing  {nameof(HandleSync)} with {jobId}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation($"Cancelled {nameof(HandleSync)} with {jobId}");
                throw;
            }
        }

        public async Task RecurringSync(Func<Task> methodCall)
        {
            try
            {

            }
            catch (Exception)
            {

                throw;
            }
        }  

    }
}
