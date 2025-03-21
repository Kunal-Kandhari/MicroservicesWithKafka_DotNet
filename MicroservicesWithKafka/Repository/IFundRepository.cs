﻿using MicroservicesWithKafka.Models;

namespace MicroservicesWithKafka.Repository
{
    public interface IFundRepository
    {
        Task<List<Fund>> GetAllFunds();
        Task<(List<Fund> Items, int TotalCount)> GetPagedFunds(int page, int pageSize);
        Task<Fund> GetFundByID(int id);
        Task<List<Fund>> FilterFundsByField(string fieldName, string fieldValue);
        void AddFund(Fund fund);
        void UpdateFund(Fund fund);
        void DeleteFund(int id);
    }
}
