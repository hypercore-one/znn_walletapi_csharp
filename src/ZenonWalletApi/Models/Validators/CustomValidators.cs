using FluentValidation;

namespace ZenonWalletApi.Models.Validators
{
    public static class CustomValidators
    {
        public static IRuleBuilderOptions<T, string> WalletPassword<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .MinimumLength(8)
                .MaximumLength(255)
                .Matches("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[@$!%*#?&])[A-Za-z\\d@$!%*#?&]{8,}$");
        }

        public static IRuleBuilderOptions<T, string> WalletAmount<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .Matches("^((\\d+(\\.\\d*)?)|(\\.\\d+))$");
        }
    }
}
