using System;
using FastMiddleware.Interfaces;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace FastMiddleware.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFastMiddleware(this IServiceCollection services, params object[] args)
        {
            var assemblys = ResolveAssemblys(args);
            services.AddSingleton<IFastMiddleware, FastMiddleware>();

            RegisterHandlers(services, assemblys, typeof(IRequestHandler<,>));
            RegisterHandlers(services, assemblys, typeof(INotificationHandler<>));

            return services;
        }

        private static void RegisterHandlers(IServiceCollection services, object assemblys, Type handlerInterfaceType)
        {
            if (assemblys is Assembly[] assemblies)
            {
                var types = assemblies
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .ToList();

                    foreach (var type in types)
                    {
                        var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterfaceType);

                        foreach (var iface in interfaces)
                        {
                            services.AddTransient(iface, type);
                        }
                    }
            }
            else
            {
                throw new ArgumentException("Invalid assemblys provided. Expected an array of Assembly.");
            }
        }

        private static Assembly[] ResolveAssemblys(object[] args)
        {
            if (args == null || args.Length == 0)
            {
                return AppDomain.
                CurrentDomain.
                GetAssemblies().
                Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.FullName)).
                ToArray();
            }

            if (args.All(a => a is Assembly))
            {
                return args.Cast<Assembly>().ToArray();
            }

            if (args.All(a => a is string))
            {
                var prefixes = args.Cast<string>().ToArray();

                return AppDomain.
                CurrentDomain.
                GetAssemblies().
                Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.FullName) &&
                prefixes.Any(p => a.FullName!.StartsWith(p))).
                ToArray();
            }
            
            throw new ArgumentException("Invalid arguments provided. Expected Assembly or string prefixes.");
        }
    }
}