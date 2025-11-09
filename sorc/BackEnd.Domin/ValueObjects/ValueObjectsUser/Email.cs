using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BackEnd.Domin.ValueObjects.ValueObjectsUser
{
    [Owned]
    public class Email
    {
        public string Value { get; private set; }

        private static readonly HashSet<string> AllowedDomains = new(StringComparer.OrdinalIgnoreCase)
        {
            "gmail.com", "yahoo.com", "outlook.com", "hotmail.com",
            "icloud.com", "protonmail.com", "live.com", "msn.com",
            "aol.com", "zoho.com", "mail.com", "yandex.com"
        };

        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ✅ Constructor خاص لـ EF Core - بدون validation
        private Email() { }

        // ✅ Constructor خاص داخلي - يضع القيمة مباشرة
        private Email(string value)
        {
            Value = value;
        }

        // ✅ Factory Method للاستخدام في الكود
        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");

            value = value.Trim().ToLower();

            if (value.Length > 254)
                throw new ArgumentException("Email is too long.");

            if (!EmailRegex.IsMatch(value))
                throw new ArgumentException("Invalid email format.");

            if (ContainsDangerousCharacters(value))
                throw new ArgumentException("Email contains invalid characters.");

            try
            {
                var mail = new MailAddress(value);
                var domain = mail.Host;

                if (!IsValidDomain(domain))
                    throw new ArgumentException("Invalid email domain.");

                if (!IsAllowedDomain(domain))
                    throw new ArgumentException($"Email domain '{domain}' is not allowed. Please use a recognized email provider.");

                if (IsDisposableEmail(domain))
                    throw new ArgumentException("Disposable email addresses are not allowed.");

                return new Email(value);
            }
            catch (FormatException)
            {
                throw new ArgumentException("Invalid email format.");
            }
        }

        private static bool IsValidDomain(string domain)
        {
            if (!domain.Contains("."))
                return false;

            var parts = domain.Split('.');

            if (parts.Last().Length < 2)
                return false;

            if (domain.StartsWith(".") || domain.EndsWith(".") ||
                domain.StartsWith("-") || domain.EndsWith("-"))
                return false;

            return true;
        }

        private static bool IsAllowedDomain(string domain)
        {
            return AllowedDomains.Contains(domain);
        }

        private static bool ContainsDangerousCharacters(string email)
        {
            var dangerousChars = new[] { '<', '>', '"', '\'', ';', '(', ')', '{', '}', '[', ']', '\\', '/' };
            return email.Any(c => dangerousChars.Contains(c));
        }

        private static bool IsDisposableEmail(string domain)
        {
            var disposableDomains = new[]
            {
                "tempmail.com", "10minutemail.com", "guerrillamail.com",
                "mailinator.com", "throwaway.email", "temp-mail.org",
                "getnada.com", "maildrop.cc", "fakeinbox.com",
                "trashmail.com", "mytrashmail.com"
            };

            return disposableDomains.Contains(domain.ToLower());
        }

        public static implicit operator string(Email email) => email?.Value;

        public override bool Equals(object obj) =>
            obj is Email other && Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode() =>
            StringComparer.OrdinalIgnoreCase.GetHashCode(Value);

        public override string ToString() => Value;
    }
}