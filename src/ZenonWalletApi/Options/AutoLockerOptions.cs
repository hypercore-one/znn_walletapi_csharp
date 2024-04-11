namespace ZenonWalletApi.Options
{
    public class AutoLockerOptions
    {
        public const string AutoLocker = "Api:AutoLocker";

        public bool Enabled { get; set; } = true;

        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromMinutes(5);

        public TimeSpan TimerInterval { get; set; } = TimeSpan.FromSeconds(5);
    }
}
