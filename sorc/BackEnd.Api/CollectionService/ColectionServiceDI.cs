using BackEnd.Application.ApplicationServices.Autho;
using BackEnd.Application.Common;
using BackEnd.Application.Implementation.Autho;
using BackEnd.Application.Implementation.Common;
using BackEnd.Infrastructure.Implementation;
using BackEnd.Infrastructure.Interface;

namespace BackEnd.Api.CollectionService
{
    public static class ColectionServiceDI
    {
        public static IServiceCollection AddScopedServise(this IServiceCollection services)
        {
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IAuthentication, AuthenticationService>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<IFileService,FileService>();

            return services;
        }
        public static IServiceCollection AddTransientServise(this IServiceCollection services)
        {
            services.AddTransient<ISecurtyService, SecurtyService>();
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
        public static IServiceCollection AddSingletonServise(this IServiceCollection services)
        {

            return services;
        }
    }
}
