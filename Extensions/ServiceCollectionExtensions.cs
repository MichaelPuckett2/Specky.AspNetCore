using Microsoft.Extensions.DependencyInjection;
using Specky.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Specky.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseSpecky(this IServiceCollection serviceCollection, IEnumerable<Assembly> assemblies = null)
        {
            var callingAssembly = Assembly.GetCallingAssembly();
            SpeckyAutoStrapper.Start(assemblies ?? new[] { callingAssembly });

            SpeckyContainer
                .Instance
                .InjectedSpecks
                .Select(x => x.Type)
                .ForEach(type => type
                .GetInterfaces()
                .ForEach(intface => serviceCollection.AddSingleton(intface, x => SpeckyContainer.Instance.GetSpeck(type))));                

            return serviceCollection;
        }
    }
}
