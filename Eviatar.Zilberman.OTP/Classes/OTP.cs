using MongoDBService.Classes;

namespace securelogic.otp.Classes
{
    public sealed class OTP : AMongoDBItem
    {
        public string Value { get; set; } = string.Empty;
        public string UserNamedId { get; set; } = string.Empty;
        public int Status { get; set; } = 1;
        public DateTime Complete { get; set; } = DateTime.MinValue;
    }
}