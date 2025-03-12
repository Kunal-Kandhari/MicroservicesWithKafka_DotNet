using MicroservicesWithKafka.Models;
using System.Xml;
using Serilog;
using Newtonsoft.Json;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System;


namespace MicroservicesWithKafka.Repository
{
    public class FundRepository : IFundRepository
    {
        private readonly string filePath = "Data/fundDetails.json";

        private readonly IMongoCollection<Fund> _fundsCollection;

        public FundRepository(IOptions<FundsDatabaseSettings> fundsDatabaseSettings)
        {
            var mongoClient = new MongoClient(fundsDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(fundsDatabaseSettings.Value.DatabaseName);

            _fundsCollection = mongoDatabase.GetCollection<Fund>(fundsDatabaseSettings.Value.CollectionName);
        }

        public Task<List<Fund>> GetAllFunds()
        {
            Log.Information("Fetching all funds from JSON file.");

            return _fundsCollection.Find(_ => true).ToListAsync();

            //return Task.FromResult(ReadFromFile());
        }

        public Task<Fund> GetFundByID(int id)
        {
            Log.Information($"Fetching fund with ID: {id}");

            return _fundsCollection.Find(x => x.FundId == id).FirstOrDefaultAsync();

            //return Task.FromResult(ReadFromFile().Find(f => f.FundId == id));
        }

        public void AddFund(Fund fund)
        {
            //var funds = ReadFromFile();
            //funds.Add(fund);
            //WriteToFile(funds);

            _fundsCollection.InsertOneAsync(fund);

            Log.Information($"Fund added: {JsonConvert.SerializeObject(fund)}");
        }

        public void UpdateFund(Fund fund)
        {
            //var funds = ReadFromFile();
            //var index = funds.FindIndex(f => f.FundId == fund.FundId);
            //if (index != -1)
            //{
            //    funds[index] = fund;
            //    WriteToFile(funds);
            //    Log.Information($"Fund updated: {JsonConvert.SerializeObject(fund)}");
            //}


            if (_fundsCollection.Find(x => x.FundId == fund.FundId).FirstOrDefault() != null)
            {
                _fundsCollection.ReplaceOneAsync(x => x.FundId == fund.FundId, fund);

                Log.Information($"Fund updated: {JsonConvert.SerializeObject(fund)}");
            }
        }

        public void DeleteFund(int id)
        {
            //var funds = ReadFromFile();
            //funds.RemoveAll(f => f.FundId == id);
            //WriteToFile(funds);

            _fundsCollection.DeleteOneAsync(x => x.FundId == id);

            Log.Information($"Fund deleted with ID: {id}");
        }

        private List<Fund> ReadFromFile()
        {
            if (!File.Exists(filePath)) return new List<Fund>();
            var jsonData = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<List<Fund>>(jsonData) ?? new List<Fund>();
        }

        private void WriteToFile(List<Fund> funds)
        {
            var jsonData = JsonConvert.SerializeObject(funds, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(filePath, jsonData);
        }
    }
}
