using Microsoft.Extensions.DependencyInjection;
using Specky.Dtos;
using Specky.Enums;
using Specky.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Specky.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseSpecky(this IServiceCollection serviceCollection, string configuration = "", IEnumerable<Assembly> assemblies = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            SpeckyAutoStrapper.Start(assemblies ?? new[] { callingAssembly }, configuration);

            SpeckyContainer
                .Instance
                .InjectedSpecks
                .ForEach(speckDto =>
                {
                    InjectSpeckType(serviceCollection, speckDto.DeliveryMode, speckDto.Type);
                    speckDto.Type.GetInterfaces().ForEach(x => InjectSpeckType(serviceCollection, speckDto.DeliveryMode, x));
                });

            return serviceCollection;
        }

        private static void InjectSpeckType(IServiceCollection serviceCollection, DeliveryMode deliveryMode, Type type, Type implementationType = null)
        {
            switch (deliveryMode)
            {
                case DeliveryMode.SingleInstance:
                case DeliveryMode.Singleton:
                    serviceCollection.AddSingleton(type, x => SpeckyContainer.Instance.GetSpeck(implementationType ?? type));
                    break;
                case DeliveryMode.PerRequest:
                case DeliveryMode.Scoped:
                    serviceCollection.AddScoped(type, x => SpeckyContainer.Instance.GetSpeck(implementationType ?? type));
                    break;
                case DeliveryMode.DataSet:
                case DeliveryMode.Transient:
                default:
                    serviceCollection.AddTransient(type, x => SpeckyContainer.Instance.GetSpeck(implementationType ?? type));
                    break;
            }
        }
    }
}
