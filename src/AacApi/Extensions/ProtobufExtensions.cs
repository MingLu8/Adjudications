using Google.Protobuf.WellKnownTypes;
using Google.Type;

namespace AacApi.Extensions
{
    public static class ProtobufExtensions
    {
        // 1. Using Google.Type.Date
        public static Google.Type.Date ToProtoDate(this DateOnly date) => new()
        {
            Year = date.Year,
            Month = date.Month,
            Day = date.Day
        };

        // 2. Using Google.Type.Decimal
        // Note: Google's Decimal uses a string representation for 'value' 
        // to ensure lossless precision across different languages.
        public static Google.Type.Decimal ToProtoDecimal(this decimal value) => new()
        {
            Value = value.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        // 3. Using Google.Protobuf.Timestamp
        public static Google.Type.DateTime ToProtoDateTime(this System.DateTime dateTime)
        {
            var offset = new DateTimeOffset(dateTime).Offset;

            return new Google.Type.DateTime
            {
                Year = dateTime.Year,
                Month = dateTime.Month,
                Day = dateTime.Day,
                Hours = dateTime.Hour,
                Minutes = dateTime.Minute,
                Seconds = dateTime.Second,
                // Converting TimeSpan offset to Google's Duration format
                UtcOffset = new Google.Protobuf.WellKnownTypes.Duration
                {
                    Seconds = (long)offset.TotalSeconds
                }
            };
        }
    }
}