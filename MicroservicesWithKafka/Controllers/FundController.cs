using Confluent.Kafka;
using MicroservicesWithKafka.DTO;
using MicroservicesWithKafka.Models;
using MicroservicesWithKafka.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MicroservicesWithKafka.Controllers
{
    [Route("api/funds")]
    [ApiController]
    public class FundController : ControllerBase
    {
        private readonly BaseService _baseService;
        private readonly IEncryptionService _encryptionService;

        public FundController(BaseService baseService, IEncryptionService encryptionService)
        {
            _baseService = baseService;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFunds()
        {
            //string encryptedField = _encryptionService.Encrypt("FundName");

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

        [HttpGet("filter")]
        public async Task<IActionResult> FilterFunds([FromQuery] string field, [FromQuery] string value)
        {
            if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value))
            {
                return BadRequest(new { message = "Field and value parameters are required." });
            }

            // The field parameter has already been decrypted by the middleware
            Serilog.Log.Information($"Filtering funds by field: {field} with value: {value}");

            var fundEventDTO = new GenericEventDTO<Fund>
            {
                EventType = "FILTER_BY_FIELD",
                Type = "fund",
                Data = new Fund()
            };

            var eventData = JsonConvert.SerializeObject(fundEventDTO);
            var jsonObject = JObject.Parse(eventData);

            jsonObject["Field"] = field;
            jsonObject["Value"] = value;


            if (string.IsNullOrEmpty(field) || string.IsNullOrEmpty(value))
            {
                Log.Error("Field or Value is missing in the filter request");
                return StatusCode(500, new { message = "Field or Value is missing in the filter request" });
            }

            var updatedEventData = jsonObject.ToString();

            try
            {
                var result = await _baseService.HandleEvent<object>(updatedEventData) as Task<List<Fund>>;

                if (result != null)
                {
                    var items = await result;

                    if (items == null || items.Count == 0)
                    {
                        return NotFound(new { message = "No funds found matching the criteria." });
                    }

                    return Ok(items);
                }
                else
                {
                    throw new InvalidCastException("Invalid Result.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error($"Error filtering funds: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred while filtering funds." });
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
