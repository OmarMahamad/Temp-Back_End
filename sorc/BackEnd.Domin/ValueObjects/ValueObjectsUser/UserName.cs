using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{
    public class UserName
    {
        public string Value { get; }

        private UserName() { }

        public UserName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");
            if (value.Length < 3)
                throw new ArgumentException("Name must be at least 3 characters long.");

            Value = value.Trim();
        }

        public override string ToString() => Value;
    }

}
