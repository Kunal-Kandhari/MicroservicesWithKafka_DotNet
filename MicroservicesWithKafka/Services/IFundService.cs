using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Services
{
    public interface IFundService
    {
        List<Fund> GetFunds();
        Fund GetFund(int id);
        Task AddFund(Fund fund);
        Task UpdateFund(Fund fund);
        Task DeleteFund(int id);
    }
}
