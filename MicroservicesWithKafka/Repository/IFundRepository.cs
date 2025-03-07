using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Repository
{
    public interface IFundRepository
    {
        Task<List<Fund>> GetAllFunds();
        Task<Fund> GetFundByID(int id);
        void AddFund(Fund fund);
        void UpdateFund(Fund fund);
        void DeleteFund(int id);
    }
}
