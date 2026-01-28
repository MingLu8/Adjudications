namespace AacApi.Extensions
{
    public static class ProtobufExtensions
    {

        public static ProtoDate ToProtoDate(this DateOnly date) => new()
        {
            Year = date.Year,
            Month = date.Month,
            Day = date.Day
        };

        public static ProtoDecimal ToProtoDecimal(this decimal value) => new ProtoDecimal 
        { 
            Units = (long) decimal.Truncate(value), 
            Nanos = (int) ((value - decimal.Truncate(value)) * 1_000_000_000m) 
        };

        public static ProtoDateTime ToProtoDateTime(this DateTime dateTime)
            => new ProtoDateTime
            {
                Seconds = new DateTimeOffset(dateTime).ToUnixTimeSeconds()
            };
    }
}
