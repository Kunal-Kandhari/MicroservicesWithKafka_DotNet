using Confluent.Kafka;
using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Services;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                Type = "fund",
                Data = new Fund()
            };

            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = JsonConvert.SerializeObject(fundEventDTO)
            };

            var funds = await _baseService.HandleEvent<object>(message.Value) as Task<List<Fund>>;

            if (funds != null)
            {
                var fundList = await funds;

                if (fundList == null || fundList.Count == 0)
                {
                    return NotFound(new { message = "No funds found." });
                }

                return Ok(fundList);                  
            }
            else
            {
                throw new InvalidCastException("Invalid Result.");
            }
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPagedFunds([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest(new { message = "Page and pageSize must be greater than 0." });
            }

            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "GET_PAGED_ENTITIES",
                Type = "fund",
                Data = new Fund()
            };

            var eventData = JsonConvert.SerializeObject(fundEventDTO);

            var jsonObject = JObject.Parse(eventData);

            jsonObject["Page"] = page;
            jsonObject["PageSize"] = pageSize;

            var updatedEventData = jsonObject.ToString();

            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = updatedEventData
            };

            var funds = await _baseService.HandleEvent<object>(message.Value) as Task<(List<Fund> Items, int TotalCount)>;

            if (funds != null)
            {
                var (items, totalCount) = await funds;

                if (items == null || items.Count == 0)
                {
                    return NotFound(new { message = "No funds found." });
                }

                return Ok(new
                            {
                                items,
                                totalCount,
                                currentPage = page,
                                pageSize = pageSize,
                                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                            });
            }
            else
            {
                throw new InvalidCastException("Invalid Result.");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFund(int id)
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "GET_BY_ID",
                Type = "fund",
                Data = new Fund { FundId = id }
            };

            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = JsonConvert.SerializeObject(fundEventDTO)
            };

            var result = await _baseService.HandleEvent<object>(message.Value) as Task<Fund>;

            if (result != null)
            {
                var fund = await result;

                return fund != null ? Ok(fund) : NotFound();
            }
            else
            {
                throw new InvalidCastException("Invalid Result.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddFund([FromBody] Fund fund)
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "CREATE",
                Type = "fund",
                Data = fund
            };
            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = JsonConvert.SerializeObject(fundEventDTO)
            };

            await _baseService.HandleEvent<object>(message.Value);
            return CreatedAtAction("GetFund", new { id = fund.FundId }, fund);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFund(int id, [FromBody] Fund fund)
        {
            if (id != fund.FundId) return BadRequest("Fund ID mismatch.");

            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "UPDATE",
                Type = "fund",
                Data = fund
            };
            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = JsonConvert.SerializeObject(fundEventDTO)
            };

            await _baseService.HandleEvent<object>(message.Value);

            return Ok(new { message = "Fund updated successfully." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFund(int id)
        {
            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "DELETE",
                Type = "fund",
                Data = new Fund { FundId = id }
            };

            var message = new Message<string, string>
            {
                Key = fundEventDTO.EventType,
                Value = JsonConvert.SerializeObject(fundEventDTO)
            };

            await _baseService.HandleEvent<object>(message.Value);

            return NoContent();
        }
    }
}
