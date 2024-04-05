using Microsoft.Extensions.Options;

namespace ZenonWalletApi.Services
{
    public interface IAutoLockService : IHostedService, IDisposable
    {
        void Activity();

        void Suspend();

        void Resume();
    }

    public class AutoLockServiceOptions
    {
        public const string AutoLocker = "Api:AutoLocker";

        public bool Enabled { get; set; } = true;
        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(5);
    }

    public class AutoLockService : BackgroundService, IAutoLockService
    {
        private volatile int _counter = 0;

        public AutoLockService(ILogger<AutoLockService> logger, IOptions<AutoLockServiceOptions> options, IServiceProvider services)
        {
            Logger = logger;
            Options = options.Value;
            Enabled = Options.Enabled;
            Services = services;
        }

        private ILogger Logger { get; }

        private AutoLockServiceOptions Options { get; }

        private IServiceProvider Services { get; set; }

        public bool Enabled { get; private set; }

        public DateTime LastActivity { get; private set; }

        public void Activity()
        {
            Logger.LogDebug("Activity");
            LastActivity = DateTime.Now;
        }

        public void Suspend()
        {
            if (Interlocked.Increment(ref _counter) == 1)
            {
                Logger.LogDebug("Suspend");
                Enabled = false;
            }
        }

        public void Resume()
        {
            if (_counter > 0)
            {
                if (Interlocked.Decrement(ref _counter) == 0)
                {
                    Logger.LogDebug("Resume");
                    Enabled = true;
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var wallet = Services.GetRequiredService<IWalletService>();

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                if (Enabled &&
                    wallet.IsUnlocked &&
                    LastActivity + Options.LockTimeout < DateTime.Now)
                {
                    Logger.LogDebug("AutoLock");
                    await wallet.LockAsync();
                }
            }
        }
    }
}
