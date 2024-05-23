using System.Text.Json;
using System.Text.Json.Serialization;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models.Converters
{
    public class HashJsonConverter : JsonConverter<Hash>
    {
        public override Hash Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                return Hash.Parse(reader.GetString()!);
            }
            catch
            {
                return Hash.Empty;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            Hash hash,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(hash.ToString());
        }
    }
}
