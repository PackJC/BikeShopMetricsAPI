
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MetricsAPI.Services;

namespace MetricsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BikeShopController : ControllerBase
    {
        IMetricsRepository metricsRepository;
        public BikeShopController(IMetricsRepository _metricsRepository)
        {
            metricsRepository = _metricsRepository;
        }


        [HttpGet("{table}")] //allow access to the desired table
        public ActionResult GetBicycleList([FromRoute] string table)
        {
            var result = metricsRepository.GetTable(table);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("{table}/{id}")]
        public ActionResult GetBicycleDetails([FromRoute] string table, [FromRoute] string id)
        {
            var result = metricsRepository.GetTableItem(table, id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}