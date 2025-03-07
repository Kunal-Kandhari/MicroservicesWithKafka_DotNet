using MicroservicesWithKafka.Models;
using System.Xml;
using Serilog;
using Newtonsoft.Json;


namespace MicroservicesWithKafka.Repository
{
    public class FundRepository : IFundRepository
    {
        private readonly string filePath = "Data/fundDetails.json";

        public Task<List<Fund>> GetAllFunds()
        {
            Log.Information("Fetching all funds from JSON file.");
            return Task.FromResult(ReadFromFile());
        }

        public Task<Fund> GetFundByID(int id)
        {
            Log.Information($"Fetching fund with ID: {id}");
            return Task.FromResult(ReadFromFile().Find(f => f.FundId == id));
        }

        public void AddFund(Fund fund)
        {
            var funds = ReadFromFile();
            funds.Add(fund);
            WriteToFile(funds);
            Log.Information($"Fund added: {JsonConvert.SerializeObject(fund)}");
        }

        public void UpdateFund(Fund fund)
        {
            var funds = ReadFromFile();
            var index = funds.FindIndex(f => f.FundId == fund.FundId);
            if (index != -1)
            {
                funds[index] = fund;
                WriteToFile(funds);
                Log.Information($"Fund updated: {JsonConvert.SerializeObject(fund)}");
            }
        }

        public void DeleteFund(int id)
        {
            var funds = ReadFromFile();
            funds.RemoveAll(f => f.FundId == id);
            WriteToFile(funds);
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
