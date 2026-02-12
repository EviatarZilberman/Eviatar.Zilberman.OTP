using MongoDB.Driver;
using MongoDBService.Classes;
using securelogic.otp.Classes;
using securelogic.otp.Enums;

namespace securelogic.otp.core
{
    public sealed class OTPManager : MongoDBServiceManager<OTP>
    {
        private const string Upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string Lower = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string Special = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        private static int ValidityInSeconds = 600;
        private static int CodeLength = 6;
        private static int UpperAmount = 1;
        private static int LowerAmount = 1;
        private static int DigitAmount = 1;
        private static int SpecialAmount = 1;
        private static Types[] DefaultChars = new Types[1] { Types.LowerCase };
        private static int AddHours = 3;

        public OTPManager() { }

        public new string GetCollectionName()
        {
            return "otps";
        }

        public static void Init(string? dbName = null, int validityInSeconds = 600, int otpLength = 6, int upperAmount = 1, int lowerAmount = 1, int digitAmount = 1, int specialAmount = 1, int addHours = 3, Types[]? defaultChars = null)
        {
            ValidityInSeconds = validityInSeconds;
            CodeLength = otpLength;
            UpperAmount = upperAmount;
            LowerAmount = lowerAmount;
            DigitAmount = digitAmount;
            SpecialAmount = specialAmount;
            AddHours = addHours;
            if (defaultChars != null) DefaultChars = defaultChars;
        }

        public OTP Generate(string userNamedId)
        {
            OTP otp = new OTP()
            {
                UserNamedId = userNamedId,
                Value = GenerateValue(),
                CreationDate = DateTime.UtcNow.AddHours(AddHours)
            };
            return otp;
        }

        public bool IsValid(string value, string userNamedId)
        {
            bool res = false;
            OTP? otp = null;
            if (this.Validate(value, userNamedId, out otp))
            {
                //otp!.Status = (int)OTPStatus.Completed;
                //otp!.Complete = DateTime.Now;
                //this.Update(otp);
                res = true;
            }
            //otp!.Status = (int)OTPStatus.Used;
            //otp!.Complete = DateTime.Now;
            //this.Update(otp);
            this.Delete(otp!.Id);
            return res;
        }

        public bool GetByUserNamedId(string userNamedId, out OTP item) => this.FindOneByProperty("UserNamedId", userNamedId, out item);

        private string GenerateValue()
        {
            char[] otp = new char[CodeLength];
            Types?[] template = CreateTemplate();
            for (int i = 0; i < template.Length; i++)
            {
                otp[i] = SetOne(template[i]);
            }
            return new string(otp);
        }

        private bool Validate(string value, string userNamedId, out OTP? otp)
        {
            if (!this.GetByUserNamedId(userNamedId, out otp)) return false;
            if (otp.Value == value)
            {
                DateTime dt = otp.CreationDate.AddSeconds(ValidityInSeconds);
                if (dt >= DateTime.Now)
                {
                    if (otp.Status == (int)OTPStatus.Ready)
                    {
                        return true;
                    }
                }
            }
            otp = new OTP();
            return false;
        }

        private static Types?[] CreateTemplate()
        {
            int upper = 0;
            int lower = 0;
            int digit = 0;
            int special = 0;
            Types?[] template = new Types?[CodeLength];
            Random rdm = new Random();
            int type = -1;
            int index = 0;
            for (; index < CodeLength; index++)
            {
                while (template[index] == null)
                {
                    type = rdm.Next(0, Enum.GetValues(typeof(Types)).Length);
                    if (type == (int)Types.UpperCase && upper < UpperAmount)
                    {
                        template[index] = Types.UpperCase;
                        upper++;
                        continue;
                    }
                    if (type == (int)Types.LowerCase && lower < LowerAmount)
                    {
                        template[index] = Types.LowerCase;
                        lower++;
                        continue;
                    }
                    if (type == (int)Types.Digit && digit < DigitAmount)
                    {
                        template[index] = Types.Digit;
                        digit++;
                        continue;
                    }
                    if (type == (int)Types.Special && special < SpecialAmount)
                    {
                        template[index] = Types.Special;
                        special++;
                        continue;
                    }
                }
                if (upper == UpperAmount && lower == LowerAmount && digit == DigitAmount && special == SpecialAmount) break;
            }
            if (!AllFull(template))
            {
                index++;
                for (; index < CodeLength; index++)
                {
                    template[index] = FillFromDefaults();
                }
            }

            return template;
        }

        private static bool AllFull(Types?[] template)
        {
            return template != null && template.All(t => t != null);
        }

        private static Types FillFromDefaults()
        {
            Random rdm = new Random();
            int index = rdm.Next(0, DefaultChars.Length);
            return DefaultChars[index];

        }

        private static char SetOne(Types? types)
        {
            Random rdm = new Random();
            switch (types)
            {
                case Types.UpperCase:
                    return Upper[rdm.Next(0, Upper.Length)];
                case Types.LowerCase:
                    return Lower[rdm.Next(0, Lower.Length)];
                case Types.Digit:
                    return Digits[rdm.Next(0, Digits.Length)];
                case Types.Special:
                    return Special[rdm.Next(0, Special.Length)];
            }
            throw new Exception("Cannot set char!");
        }
    }
}