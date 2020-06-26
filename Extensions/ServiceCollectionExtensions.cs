using Microsoft.Extensions.DependencyInjection;
using Specky.Extensions;
using System.Collections.Generic;
using System.Linq;
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
                .Select(x => x.Type)
                .ForEach(type =>
                {
                    var speckDto = SpeckyContainer.Instance.InjectedSpecks.FirstOrDefault(x => x.Type == type);

                    type.GetInterfaces()
                        .ForEach(intface =>
                        {
                            switch (speckDto.DeliveryMode)
                            {
                                case Enums.DeliveryMode.SingleInstance:
                                case Enums.DeliveryMode.Singleton:
                                    serviceCollection.AddSingleton(intface, x => SpeckyContainer.Instance.GetSpeck(type));
                                    break;
                                case Enums.DeliveryMode.PerRequest:
                                case Enums.DeliveryMode.Scoped:
                                    serviceCollection.AddScoped(intface, x => SpeckyContainer.Instance.GetSpeck(type));
                                    break;
                                case Enums.DeliveryMode.DataSet:
                                case Enums.DeliveryMode.Transient:
                                default:
                                    serviceCollection.AddTransient(intface, x => SpeckyContainer.Instance.GetSpeck(type));
                                    break;
                            }
                        });
                });

            return serviceCollection;
        }
    }
}
