using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Services
{
    public interface IBaseService<T>
    {
        //Task GetEntity();
        Task AddEntity(T entity);
        Task UpdateEntity(T entity);
        Task DeleteEntity(int entityId);
    }
}
