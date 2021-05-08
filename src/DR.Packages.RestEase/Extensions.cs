using System;
using System.Linq;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using RestEase;

namespace DR.Packages.RestEase
{
    public static class Extensions
    {
        public static void RegisterServiceForwarder<T>(
            this IServiceCollection services,
            string serviceName,
            string serviceUrl,
            Enumeration.Lifestyle lifestyle = Enumeration.Lifestyle.Transient,
            ResponseDeserializer deserializer = null)
            where T : class
        {
            var clientName = typeof(T).ToString();

            ConfigureDefaultClient(services, clientName, serviceName, new RestEaseOptions(new Uri(serviceUrl), serviceName));

            ConfigureForwarder<T>(services, clientName, lifestyle, deserializer);
        }

        public static void RegisterServiceForwarder<T>(
            this IServiceCollection services,
            string serviceName,
            RestEaseOptions options,
            Enumeration.Lifestyle lifestyle = Enumeration.Lifestyle.Transient,
            ResponseDeserializer deserializer = null)
            where T : class
        {
            var clientName = typeof(T).ToString();

            ConfigureDefaultClient(services, clientName, serviceName, options);

            ConfigureForwarder<T>(services, clientName, lifestyle, deserializer);
        }


        private static void ConfigureDefaultClient(IServiceCollection services, string clientName,
            string serviceName, RestEaseOptions options)
        {
            services.AddHttpClient(clientName, client =>
            {
                var service = options.Services.SingleOrDefault(s => s.Name.Equals(serviceName,
                    StringComparison.InvariantCultureIgnoreCase));
                if (service == null)
                {
                    throw new ArgumentNullException(serviceName,
                        $"Rest service: '{serviceName}' was not found.");
                }

                var builder = new UriBuilder();
                builder.Scheme = service.Scheme;
                builder.Host = service.Host;

                if (service.Port > 0)
                {
                    builder.Port = service.Port;
                }

                client.BaseAddress = new Uri(builder
                    .Uri
                    .ToString() +
                    (string.IsNullOrEmpty(service.Prefix) ? string.Empty : service.Prefix + "/"));
            });
        }

        private static void ConfigureForwarder<T>(
            IServiceCollection services,
            string clientName,
            Enumeration.Lifestyle lifestyle = Enumeration.Lifestyle.Transient,
            ResponseDeserializer deserializer = null)
            where T : class
        {
            if (lifestyle == Enumeration.Lifestyle.Transient)
            {
                if (deserializer != null)
                {
                    services.AddTransient<T>(c => new RestClient(c.GetService<IHttpClientFactory>().CreateClient(clientName))
                    {
                        RequestQueryParamSerializer = new QueryParamSerializer(),
                        ResponseDeserializer = deserializer
                    }.For<T>());
                }
                else
                {
                    services.AddTransient<T>(c => new RestClient(c.GetService<IHttpClientFactory>().CreateClient(clientName))
                    {
                        RequestQueryParamSerializer = new QueryParamSerializer()
                    }.For<T>());
                }
            }
            else if (lifestyle == Enumeration.Lifestyle.Scoped)
            {
                services.AddScoped<T>(c => new RestClient(c.GetService<IHttpClientFactory>().CreateClient(clientName))
                {
                    RequestQueryParamSerializer = new QueryParamSerializer()
                }.For<T>());
            }
            else if (lifestyle == Enumeration.Lifestyle.Singleton)
            {
                services.AddSingleton<T>(c => new RestClient(c.GetService<IHttpClientFactory>().CreateClient(clientName))
                {
                    RequestQueryParamSerializer = new QueryParamSerializer()
                }.For<T>());
            }
            else
            {
                throw new ArgumentException($"Lifestyle {lifestyle.ToString()} is not a valid lifestyle.");
            }
        }
    }
}
