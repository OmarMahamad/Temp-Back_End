using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Common.User
{
    public record UserCommandNotfication(string email, string supject, string body):INotification;
}
