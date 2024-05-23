using System.Text.Json;
using System.Text.Json.Serialization;
using Zenon.Model.Primitives;

namespace ZenonWalletApi.Models.Converters
{
    public class AddressJsonConverter : JsonConverter<Address>
    {
        public override Address Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            try
            {
                var address = Address.Parse(reader.GetString()!);
                return address;
            }
            catch
            {
                return Address.EmptyAddress;
            }
        }

        public override void Write(
            Utf8JsonWriter writer,
            Address address,
            JsonSerializerOptions options) =>
                writer.WriteStringValue(address.ToString());
    }
}
