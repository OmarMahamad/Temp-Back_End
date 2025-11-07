using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{
    public class Password
    {
        public string Hash { get; }
        public string Salt { get; }

        private Password() { }

        public Password(string hash, string salt)
        {
            if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(salt))
                throw new ArgumentException("Password data invalid.");
            Hash = hash;
            Salt = salt;
        }
    }

}
