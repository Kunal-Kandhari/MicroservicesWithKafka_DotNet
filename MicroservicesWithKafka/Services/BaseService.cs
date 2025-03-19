using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System.Reflection;
using System.Security.Principal;
using System.Text.Json;

namespace MicroservicesWithKafka.Services
{
    public class BaseService
    {
        private readonly IServiceFactory _servicefactory;

        private readonly Dictionary<string, Type> _typeMap = new Dictionary<string, Type>
                                                                                    {
                                                                                            { "fund", typeof(Fund) }
                                                                                    };



        public BaseService(IServiceFactory servicefactory)
        {
            _servicefactory = servicefactory;
        }        

        public async Task<object> HandleEvent<T>(string messageValue) where T : class
        {
            var typeInfo = ExtractInfo(messageValue, "Type");

            if (typeInfo == null || !_typeMap.TryGetValue(typeInfo, out Type entityType))
            {
                Log.Error("Unable to fetch Type from request!!!");
                return false;
            }

            var eventDTO = JsonConvert.DeserializeObject<GenericEventDTO<T>>(messageValue);

            var service = _servicefactory.GetService(messageValue);

            if (service == null)
            {
                Log.Error("Unable to fetch Service!!!");
                return null;
            }

            Type serviceType = service.GetType();

            MethodInfo processMethod;

            switch (eventDTO.EventType)
            {
                case "GET_ALL":
                    processMethod = serviceType.GetMethod("GetAllEntities");
                    if (processMethod != null)
                    {
                        return processMethod.Invoke(service, null);
                    } 
                    else
                    {
                        Log.Error($"Unable to find Method: GetAllEntities in Service: {service.ToString()}");
                    }
                    break;

                case "GET_PAGED_ENTITIES":
                    processMethod = serviceType.GetMethod("GetPagedEntities");
                    if (processMethod != null)
                    {   
                        int page = 1;
                        int pageSize = 10;

                        JObject jObject = JObject.Parse(messageValue);

                        if (jObject["Page"] != null)
                            page = jObject["Page"].Value<int>();
                        if (jObject["PageSize"] != null)
                            pageSize = jObject["PageSize"].Value<int>();

                        return processMethod.Invoke(service, new object[] { page, pageSize });
                    }
                    else
                    {
                        Log.Error($"Unable to find Method: GetPagedEntities in Service: {service.ToString()}");
                    }
                    break;

                case "GET_BY_ID":
                    processMethod = serviceType.GetMethod("GetEntityByID");
                    if (processMethod != null)
                    {
                        object id = (object)GetEntityId(JsonConvert.DeserializeObject(ExtractInfo(messageValue, "Data"), entityType));
                        return processMethod.Invoke(service, new[] { id });
                    }
                    else
                    {
                        Log.Error($"Unable to find Method: GetEntityByID in Service: {service.ToString()}");
                    }
                    break;

                case "CREATE":
                    processMethod = serviceType.GetMethod("AddEntity");
                    if (processMethod != null)
                    {
                        processMethod.Invoke(service, new[] { JsonConvert.DeserializeObject(ExtractInfo(messageValue, "Data"), entityType) });
                    }
                    else
                    {
                        Log.Error($"Unable to find Method: AddEntity in Service: {service.ToString()}");
                    }
                    break;

                case "UPDATE":
                    processMethod = serviceType.GetMethod("UpdateEntity");
                    if (processMethod != null)
                    {
                        processMethod.Invoke(service, new[] { JsonConvert.DeserializeObject(ExtractInfo(messageValue, "Data"), entityType) });
                    }
                    else
                    {
                        Log.Error($"Unable to find Method: UpdateEntity in Service: {service.ToString()}");
                    }
                    break;

                case "DELETE":
                    processMethod = serviceType.GetMethod("DeleteEntity");
                    if (processMethod != null)
                    {
                        object id = (object)GetEntityId(JsonConvert.DeserializeObject(ExtractInfo(messageValue, "Data"), entityType));
                        processMethod.Invoke(service, new[] { id });
                    }
                    else
                    {
                        Log.Error($"Unable to find Method: DeleteEntity in Service: {service.ToString()}");
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unknown event type");
            }

            return null;
        }

        private int GetEntityId<T>(T data) where T : class
        {
            if (data is Fund fund)
                return fund.FundId;

            throw new InvalidOperationException("Entity has no Id");
        }

        private string ExtractInfo(string json, string field)
        {
            try
            {
                JObject jObject = JObject.Parse(json);
                return jObject[field]?.ToString().ToLower();
            }
            catch
            {
                return null;
            }
        }

        public static bool TryDeserializeToDTO<T>(string json, out T result)
        {
            try
            {
                result = JsonConvert.DeserializeObject<T>(json);
                return result != null;
            }
            catch 
            {
                result = default;
                return false;
            }
        }

        private async Task<List<T>> GetAllEntities<T>(GenericEventDTO<T> eventDTO) where T : class
        {
            //if (eventDTO.Data is Fund)
            //{
            //    var fundService = _servicefactory.GetService<T>(eventDTO);
            //    return await fundService.GetAllEntities() as List<T>;
            //}
            
            if (TryDeserializeToDTO(JsonConvert.SerializeObject(eventDTO.Data), out Fund fundDTO))
            {
                //var fundService = _servicefactory.GetService<T>(eventDTO);
                //var fundService = _serviceProvider.GetRequiredService<FundService>() as IBaseService<T>; 
                //return await fundService.GetAllEntities() as List<T>;
            }

            throw new InvalidOperationException("No service found for fetching all entities.");
        }
    }
}
