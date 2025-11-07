using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Domin.Entity
{
    [Index(nameof(Token))]
    public class EmailVerificationToken
    {
        [Key]
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public Guid Token { get; private set; } = Guid.NewGuid();

        public DateTime ExpiryDate { get; private set; }

        public bool IsUsed { get; private set; } = false;

        // Navigation property
        [ForeignKey(nameof(UserId))]
        public User User { get; private set; }

        // EF Core يحتاج Constructor فارغ
        protected EmailVerificationToken() { }

        // Private Constructor — لا يمكن لأي كود خارجي استخدامه
        private EmailVerificationToken(Guid userId, DateTime expiryDate)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID.");

            if (expiryDate <= DateTime.UtcNow)
                throw new ArgumentException("Expiry date must be in the future.");

            Id = Guid.NewGuid();
            UserId = userId;
            ExpiryDate = expiryDate;
        }

        //  Factory Method — الطريقة الوحيدة لإنشاء الكائن
        public static EmailVerificationToken Create(Guid userId, int validMinutes = 15)
        {
            var expiryDate = DateTime.UtcNow.AddMinutes(validMinutes);
            return new EmailVerificationToken(userId, expiryDate);
        }

        //  منطق الدومين — تحقق من الصلاحية
        public bool IsValid()
        {
            return !IsUsed && DateTime.UtcNow <= ExpiryDate;
        }

        //  منطق الدومين — تعليم الرمز كمستخدم
        public void MarkAsUsed()
        {
            if (IsUsed)
                throw new InvalidOperationException("Token already used.");

            IsUsed = true;
        }
    }
}
