using Microsoft.Extensions.Options;
using ZenonWalletApi.Options;

namespace ZenonWalletApi.Services
{
    public interface IAutoLockerService : IHostedService
    {
        bool IsEnabled { get; }

        bool IsSuspended { get; }

        void Activity();

        void Suspend();

        void Resume();
    }

    internal class AutoLockerService : BackgroundService, IAutoLockerService, IDisposable
    {
        private volatile int _counter = 0;

        public AutoLockerService(ILogger<AutoLockerService> logger, IOptions<AutoLockerOptions> options, IServiceProvider services)
        {
            Logger = logger;
            Options = options.Value;
            Timer = new PeriodicTimer(Options.TimerInterval);
            Services = services;
        }

        private ILogger Logger { get; }

        private AutoLockerOptions Options { get; }

        private PeriodicTimer Timer { get; }

        private IServiceProvider Services { get; }

        public bool IsEnabled => Options.Enabled;

        public bool IsSuspended { get; private set; }

        public DateTime LastActivity { get; private set; }

        public void Activity()
        {
            Logger.LogDebug("Activity");
            LastActivity = DateTime.UtcNow;
        }

        public void Suspend()
        {
            if (Interlocked.Increment(ref _counter) == 1)
            {
                Logger.LogDebug("Suspend");
                IsSuspended = true;
            }
        }

        public void Resume()
        {
            if (_counter > 0)
            {
                if (Interlocked.Decrement(ref _counter) == 0)
                {
                    Logger.LogDebug("Resume");
                    IsSuspended = false;
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var wallet = Services.GetRequiredService<IWalletService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Timer.WaitForNextTickAsync(stoppingToken);

                if (IsEnabled && !IsSuspended && wallet.IsUnlocked &&
                    LastActivity + Options.LockTimeout < DateTime.UtcNow)
                {
                    Logger.LogDebug("AutoLock");
                    await wallet.LockAsync();
                }
            }
        }
    }
}
