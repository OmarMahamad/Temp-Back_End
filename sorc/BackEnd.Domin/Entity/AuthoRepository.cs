using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.Entity
{
    public class AuthoRepository
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        [Required]
        public string Token { get; private set; }

        [Required]
        public string Refresh { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime ExpireAt { get; private set; }

        public bool IsRevoked { get; private set; } = false;

        // Navigation Property
        [ForeignKey(nameof(UserId))]
        public User User { get; private set; }

        // EF Core Constructor
        protected AuthoRepository() { }

        // Private Constructor
        private AuthoRepository(Guid userId, string token, string refresh, DateTime expireAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user id.");

            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Access token cannot be empty.");

            if (string.IsNullOrWhiteSpace(refresh))
                throw new ArgumentException("Refresh token cannot be empty.");

            if (expireAt <= DateTime.UtcNow)
                throw new ArgumentException("Expire date must be in the future.");

            Id = Guid.NewGuid();
            UserId = userId;
            Token = token;
            Refresh = refresh;
            CreatedAt = DateTime.UtcNow;
            ExpireAt = expireAt;
        }

        // Factory Method — الطريقة الرسمية للإنشاء
        public static AuthoRepository Create(Guid userId, string token, string refresh, int validMinutes = 60)
        {
            var expiry = DateTime.UtcNow.AddMinutes(validMinutes);
            return new AuthoRepository(userId, token, refresh, expiry);
        }
        public void RotateTokens(string newToken, string newRefresh, int validMinutes = 60)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentException("New access token cannot be empty.");

            if (string.IsNullOrWhiteSpace(newRefresh))
                throw new ArgumentException("New refresh token cannot be empty.");

            Token = newToken.Trim();
            Refresh = newRefresh.Trim();
            CreatedAt = DateTime.UtcNow;
            ExpireAt = DateTime.UtcNow.AddMinutes(validMinutes);
        }

        // Domain Logic — تحقق من صلاحية التوكن
        public bool IsValid()
        {
            return !IsRevoked && DateTime.UtcNow < ExpireAt;
        }

        // Domain Logic — إلغاء التوكن
        public void Revoke()
        {
            if (IsRevoked)
                throw new InvalidOperationException("Token already revoked.");

            IsRevoked = true;
        }
    }
}
