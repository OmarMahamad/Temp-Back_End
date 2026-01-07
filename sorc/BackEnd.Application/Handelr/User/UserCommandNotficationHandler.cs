using BackEnd.Application.Common.User;
using BackEnd.Application.DTOs.Common;
using BackEnd.Application.Helper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application.Handelr.User
{
    public class UserCommandNotficationHandler : INotificationHandler<UserCommandNotfication>
    {
        private readonly IEmailService emailService;

        public UserCommandNotficationHandler(IEmailService emailService)
        {
            this.emailService = emailService;
        }
        public Task Handle(UserCommandNotfication notification, CancellationToken cancellationToken)
        {
            var sandemaildto = new SandEmailDTO
            {
                EmailTo = notification.email,
                Subject = notification.supject,
                Body = notification.body,
            };
            return emailService.SendEmailAsync(sandemaildto);
        }
    }
}
