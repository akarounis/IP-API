namespace NovibetProject.Services
{
    public class IpDetailsUpdaterHostedService : IHostedService, IDisposable
    {
        private readonly ILogger logger_;
        private Timer timer_;
        private readonly IServiceScopeFactory scopeFactory_;


        public IpDetailsUpdaterHostedService(IServiceScopeFactory scopeFactory, ILogger<IpDetailsUpdaterHostedService> logger)
        {
            this.scopeFactory_ = scopeFactory;
            this.logger_ = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger_.LogInformation("IpDetailsUpdaterHostedService is starting.");
            timer_ = new Timer(async state => await DoWorkAsync(state, cancellationToken), null, TimeSpan.Zero, TimeSpan.FromHours(1));

            return Task.CompletedTask;
        }

        private async Task DoWorkAsync(object state, CancellationToken cancellationToken)
        {
            logger_.LogInformation("IpDetailsUpdaterHostedService has started.");

            using (var scope = scopeFactory_.CreateScope())
            {
                var ipDetailsService = scope.ServiceProvider.GetRequiredService<IpDetailsService>();

                try
                {
                    await ipDetailsService.UpdateIpDetails();
                    logger_.LogInformation("IpDetailsUpdaterHostedService has finished updating IP details.");
                }
                catch (Exception ex)
                {
                    logger_.LogError(ex, "An error occurred while updating IP details.");
                }
            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger_.LogInformation("IpDetailsUpdaterHostedService is stopping.");
            timer_?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            timer_?.Dispose();
        }
        
    }
}
