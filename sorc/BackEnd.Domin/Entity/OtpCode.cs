using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.Entity
{
    public class OtpCode
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        [Required]
        public string Code { get; private set; }

        [Required]
        public DateTime ExpiryDate { get; private set; }

        public bool IsUsed { get; private set; } = false;

        [ForeignKey(nameof(UserId))]
        public User User { get; private set; }

        // EF Core constructor
        protected OtpCode() { }

        // Private constructor
        private OtpCode(Guid userId, string code, DateTime expiryDate)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user id.");

            if (string.IsNullOrWhiteSpace(code) || code.Length != 6)
                throw new ArgumentException("OTP code must be 6 digits.");

            if (expiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future.");

            Id = Guid.NewGuid();
            UserId = userId;
            Code = code;
            ExpiryDate = expiryDate;
        }

        //  Factory Method (الطريقة الرسمية للإنشاء)
        public static OtpCode Create(Guid userId, string code, int validMinutes = 5)
        {
            var expiry = DateTime.UtcNow.AddMinutes(validMinutes);
            return new OtpCode(userId, code, expiry);
        }

        //  Domain Logic — تحقق من صلاحية الكود
        public bool IsValid()
        {
            return !IsUsed && DateTime.UtcNow <= ExpiryDate;
        }

        //  Domain Logic — استخدام الكود
        public void MarkAsUsed()
        {
            if (IsUsed)
                throw new InvalidOperationException("OTP code already used.");

            IsUsed = true;
        }
    }
}
