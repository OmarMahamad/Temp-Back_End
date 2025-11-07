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
            return services;
        }
        public static IServiceCollection AddTransientServise(this IServiceCollection services)
        {
            services.AddTransient<ISecurtyService, SecurtyService>();
            return services;
        }
    }
}
