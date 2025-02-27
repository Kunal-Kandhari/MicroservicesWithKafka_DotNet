using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Services
{
    public class BaseService
    {
        private readonly IServiceProvider _serviceProvider;

        public BaseService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public async Task<object> HandleEvent<T>(GenericEventDTO<T> eventDTO) where T : class
        {
            IBaseService<T> service = GetService<T>();

            switch (eventDTO.EventType)
            {
                case "GET_ALL":
                    return await GetAllEntities<T>();

                case "CREATE":
                    await service.AddEntity(eventDTO.Data);
                    break;

                case "UPDATE":
                    await service.UpdateEntity(eventDTO.Data);
                    break;

                case "DELETE":
                    await service.DeleteEntity(GetEntityId(eventDTO.Data));
                    break;

                default:
                    throw new InvalidOperationException("Unknown event type");
            }

            return null;
        }


        private IBaseService<T> GetService<T>()
        {
            if (typeof(T) == typeof(Fund))
            {
                return _serviceProvider.GetRequiredService<FundService>() as IBaseService<T>;
            }

            throw new InvalidOperationException("No service found for this entity type");
        }


        private int GetEntityId<T>(T data) where T : class
        {
            if (data is Fund fund)
                return fund.FundId;

            throw new InvalidOperationException("Entity has no Id");
        }

        private async Task<List<T>> GetAllEntities<T>() where T : class
        {
            if (typeof(T) == typeof(Fund))
            {
                var fundService = _serviceProvider.GetRequiredService<FundService>();
                return await fundService.GetAllEntities() as List<T>;
            }

            throw new InvalidOperationException("No service found for fetching all entities.");
        }
    }
}
