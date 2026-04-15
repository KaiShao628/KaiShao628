using FamilyLedgeManagement.Utilities;

namespace FamilyLedgeManagement.Database
{
    /// <summary>
    /// 项目服务注册扩展。
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// 注册项目内所有服务类。
        /// </summary>
        public static IServiceCollection AddProjectServices(this IServiceCollection services)
        {
            var assembly = typeof(Program).Assembly;

            var serviceTypes = assembly.GetTypes()
                .Where(t => t.Name.EndsWith("Service") && t.IsClass && !t.IsAbstract);

            foreach (var type in serviceTypes)
            {
                services.AddScoped(type);

                foreach (var interfaceType in type.GetInterfaces())
                {
                    services.AddScoped(interfaceType, type);
                }
            }

            services.AddScoped<FamilyLedgeMessageHelper>();
            return services;
        }
    }
}
