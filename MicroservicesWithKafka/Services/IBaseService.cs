using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Services
{
    public interface IBaseService<T>
    {
        Task<List<T>> GetAllEntities();
        Task AddEntity(T entity);
        Task UpdateEntity(T entity);
        Task DeleteEntity(int entityId);
    }
}
