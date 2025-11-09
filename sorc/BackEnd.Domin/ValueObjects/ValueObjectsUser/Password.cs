using Microsoft.EntityFrameworkCore;


namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{   
    [Owned]
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
