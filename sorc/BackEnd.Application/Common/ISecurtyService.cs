using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Common
{
    public interface ISecurtyService
    {
        string HashPassword(string originalValue, out string salt);
        bool VerifyPassword(string hashedValue, string originalValue, string salt);
        string VerifyEmail(string email);
        string GenerateOtpCode(int length = 6);
        string GenerateBranchCode(int length = 8);
    }
}
