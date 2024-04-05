using System.Text.Json;
using System.Text.Json.Serialization;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models.Converters
{
    public class TokenStandardJsonConverter : JsonConverter<TokenStandard>
    {
        private const string ZnnString = "ZNN";
        private const string QsrString = "QSR";

        public override TokenStandard Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                var value = reader.GetString()!;

                if (string.Equals(value, ZnnString, StringComparison.OrdinalIgnoreCase))
                {
                    return TokenStandard.ZnnZts;
                }
                else if (string.Equals(value, QsrString, StringComparison.OrdinalIgnoreCase))
                {
                    return TokenStandard.QsrZts;
                }
                else
                {
                    return TokenStandard.Parse(value);
                }
            }
            catch
            {
                return TokenStandard.EmptyZts;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            TokenStandard tokenStandard,
            JsonSerializerOptions options)
        {
            if (tokenStandard == TokenStandard.ZnnZts)
            {
                writer.WriteStringValue(ZnnString);
            }
            else if (tokenStandard == TokenStandard.QsrZts)
            {
                writer.WriteStringValue(QsrString);
            }
            else
            {
                writer.WriteStringValue(tokenStandard.ToString());
            }
        }
    }
}
