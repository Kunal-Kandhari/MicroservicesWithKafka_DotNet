using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Repository
{
    public interface IFundRepository
    {
        Task<List<Fund>> GetAllFunds();
        Fund GetFundById(int id);
        void AddFund(Fund fund);
        void UpdateFund(Fund fund);
        void DeleteFund(int id);
    }
}
