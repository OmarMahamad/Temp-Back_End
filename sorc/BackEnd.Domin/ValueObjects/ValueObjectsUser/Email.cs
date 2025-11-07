using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{
    public class Email
    {
        public string Value { get; }

        private Email() { } // For EF Core

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");

            var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(value, emailPattern))
                throw new ArgumentException("Invalid email format.");

            Value = value.Trim().ToLower();
        }

        public override bool Equals(object obj) =>
            obj is Email other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }

}
