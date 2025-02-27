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

        public Task<List<Fund>> GetAllEntities() => _fundRepository.GetAllFunds();

        //public Task GetEntity(int id) => _fundRepository.GetFundById(id);

        public async Task AddEntity(Fund fund)
        {
            _fundRepository.AddFund(fund);
            await _kafkaProducer.PublishMessage("fund-events", "CREATE", fund);
        }

        public async Task UpdateEntity(Fund fund)
        {
            _fundRepository.UpdateFund(fund);
            await _kafkaProducer.PublishMessage("fund-events", "UPDATE", fund);
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
