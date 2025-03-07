using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MicroservicesWithKafka.Services
{
    public interface IServiceFactory
    {
        IBaseService<T> GetService<T>(GenericEventDTO<T> eventDTO, Type entityType) where T : class;
        object GetService(string messageValue);
    }

    public class ServiceFactory : IServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>
                                                                                    {
                                                                                            { "fund", typeof(Fund) }
                                                                                    };

        public ServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBaseService<T> GetService<T>(GenericEventDTO<T> eventDTO, Type entityType) where T : class
        {
            if (entityType.Name == nameof(Fund))
            {
                //var ser = _serviceProvider.GetRequiredService<FundService>() as IBaseService<T>;
                //return ser;

                var serviceType = typeof(IBaseService<>).MakeGenericType(entityType);
                var ser = _serviceProvider.GetRequiredService(serviceType) as IBaseService<T>;
                return ser;
            }

            throw new InvalidOperationException("No service found for this entity type");
        }

        public object GetService(string messageValue)
        {
            try
            {
                string typeInfo = ExtractTypeInfo(messageValue);

                if (string.IsNullOrEmpty(typeInfo) || !_typeMap.TryGetValue(typeInfo, out Type entityType))
                {
                    return null;
                }

                var serviceType = typeof(IBaseService<>).MakeGenericType(entityType);
                return _serviceProvider.GetService(serviceType);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private string ExtractTypeInfo(string json)
        {
            try
            {
                // Option 1: If your JSON has a type field
                JObject jObject = JObject.Parse(json);
                return jObject["Type"]?.ToString().ToLower();
            }
            catch
            {
                return null;
            }
        }


    }
}
