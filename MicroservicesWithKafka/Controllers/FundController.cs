using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace MicroservicesWithKafka.Controllers
{
    [Route("api/funds")]
    [ApiController]
    public class FundController : ControllerBase
    {
        private readonly BaseService _baseService;

        public FundController(BaseService baseService)
        {
            _baseService = baseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFunds()
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "GET_ALL",
                Data = null 
            };

            var funds = (List<Fund>)await _baseService.HandleEvent(fundEventDTO);

            if (funds == null || funds.Count == 0)
            {
                return NotFound(new { message = "No funds found." });
            }

            return Ok(funds);
        }

        //[HttpGet("{id}")]
        //public IActionResult GetFund(int id)
        //{
        //    var fund = _fundService.GetFund(id);
        //    return fund != null ? Ok(fund) : NotFound();
        //}

        [HttpPost]
        public async Task<IActionResult> AddFund([FromBody] Fund fund)
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "CREATE",
                Data = fund
            };

            await _baseService.HandleEvent(fundEventDTO);
            return CreatedAtAction("GetFund", new { id = fund.FundId }, fund);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFund(int id, [FromBody] Fund fund)
        {
            if (id != fund.FundId) return BadRequest("Fund ID mismatch.");

            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "UPDATE",
                Data = fund
            };

            await _baseService.HandleEvent(fundEventDTO);

            return Ok(new { message = "Fund updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFund(int id)
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "DELETE",
                Data = new Fund { FundId = id }
            };


            await _baseService.HandleEvent(fundEventDTO);

            return NoContent();
        }
    }
}
