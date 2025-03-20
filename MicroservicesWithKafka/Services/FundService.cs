using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Repository;
using MicroservicesWithKafka.Kafka;
using Newtonsoft.Json;
using System;

namespace MicroservicesWithKafka.Services
{
    public class FundService : IBaseService<Fund>
    {
        private readonly IFundRepository _fundRepository;
        private readonly KafkaProducer _kafkaProducer;

        public FundService(IFundRepository fundRepository, KafkaProducer kafkaProducer)
        {
            _fundRepository = fundRepository;
            _kafkaProducer = kafkaProducer;
        }

        public async Task<List<Fund>> GetAllEntities()
        {
            return await _fundRepository.GetAllFunds();
        }

        public async Task<(List<Fund> Items, int TotalCount)> GetPagedEntities(int page, int pageSize)
        {
            return await _fundRepository.GetPagedFunds(page, pageSize);
        }

        public async Task<Fund> GetEntityByID(int id)
        {
            return await _fundRepository.GetFundByID(id);
        }

        public async Task<List<Fund>> FilterEntitiesByField(string fieldName, string fieldValue)
        {
            return await _fundRepository.FilterFundsByField(fieldName, fieldValue);
        }

        public async Task AddEntity(Fund fund)
        {
            _fundRepository.AddFund(fund);
            await _kafkaProducer.PublishMessage("fund-events", "CREATE", fund);
        }

        public async Task UpdateEntity(Fund fund)
        {
            _fundRepository.UpdateFund(fund);
            await _kafkaProducer.PublishMessage("user-events", "UPDATE", fund);
        }

        public async Task DeleteEntity(int id)
        {
            _fundRepository.DeleteFund(id);

            var fund = new Fund();
            fund.FundId = id;

            await _kafkaProducer.PublishMessage("fund-events", "DELETE", fund);
        }
    }
}
