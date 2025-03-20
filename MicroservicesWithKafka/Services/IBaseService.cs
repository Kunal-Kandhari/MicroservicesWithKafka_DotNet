using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Services
{
    public interface IBaseService<T>
    {
        Task<List<T>> GetAllEntities();
        Task<(List<T> Items, int TotalCount)> GetPagedEntities(int page, int pageSize);
        Task<T> GetEntityByID(int id);
        Task<List<Fund>> FilterEntitiesByField(string fieldName, string fieldValue);
        Task AddEntity(T entity);
        Task UpdateEntity(T entity);
        Task DeleteEntity(int entityId);
    }
}
