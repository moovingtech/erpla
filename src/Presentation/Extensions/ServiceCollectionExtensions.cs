using Core.Application.Service;
using Core.Interfaces;
using Core.Domain;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Core.Security.Application.Service;
using Core.Security.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Presentation.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>))
                //.AddScoped<IUserRepository, UserRepository>()
                //.AddScoped<IDepartmentRepository, DepartmentRepository>()
                ;
        }

        public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        {
            return services.AddScoped<IUnitOfWork, UnitOfWork>();
        }

        public static void AddDatabases(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddSqlServer<ErplaDBContext>(configuration.GetConnectionString("Erpla"))
                .AddIdentityCore<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ErplaDBContext>();

            services.AddDbContext<ErplaTangoDBContext>(options =>
                     options.UseSqlServer(configuration.GetConnectionString("ErplaTango")));

        }

        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddScoped<AuthenticationService>();
        }
    }
}