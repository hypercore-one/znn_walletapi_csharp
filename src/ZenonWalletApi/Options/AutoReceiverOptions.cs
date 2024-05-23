namespace ZenonWalletApi.Options
{
    public class AutoReceiverOptions
    {
        public const string AutoReceiver = "Api:AutoReceiver";

        public bool Enabled { get; set; } = true;

        public TimeSpan TimerInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
