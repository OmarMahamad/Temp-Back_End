using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{
    public class PhoneNumber
    {
        public string Value { get; }

        private PhoneNumber() { } // For EF

        public PhoneNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number is required.");

            var egyptPhonePattern = @"^(?:\+20|0020|0)?1[0-2,5]{1}[0-9]{8}$";
            if (!Regex.IsMatch(value, egyptPhonePattern))
                throw new ArgumentException("Invalid Egyptian phone number format.");

            Value = value;
        }

        public override string ToString() => Value;
    }

}
