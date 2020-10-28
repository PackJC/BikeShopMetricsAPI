//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:		Metrics
//	File Name:		MetricsController.cs
//	Description:	HTTP Methods for MetricsAPI
//	Course:			CSCI 4350-941 - Software Engineering II
//	Authors:		
//	Created:		Monday, October 12, 2020
//	Copyright:		
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BikeShopAPI.Repositories;
using System.Text.Json;
using Microsoft.AspNetCore.Cors;

namespace BikeShopAPI.Controllers
{
    /// <summary>
    /// Metrics Controller class. Inherits from ControllerBase
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("_myAllowSpecificOrigins")]
    public class MetricsController : ControllerBase
    {
        /// <summary>
        /// Create instance of MetricsRepository interface to access meth ods
        /// </summary>
        IMetricsRepository metricRepository;
        public MetricsController(IMetricsRepository _metricsRepository)
        {
            metricRepository = _metricsRepository;
        }

        /// <summary>
        /// READ all items in a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        [HttpGet("{table}")] //allow access to the desired table
        [EnableCors("_myAllowSpecificOrigins")]
        public ActionResult GetMetricList([FromRoute] string table)
        {
            var result = metricRepository.GetTable(table);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// READ record from table based on primary key
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{table}/{id}")]
        [EnableCors("_myAllowSpecificOrigins")]
        public ActionResult GetMetricDetails([FromRoute] string table, [FromRoute] string id)
        {
            var result = metricRepository.GetTableItem(table, id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// UPDATE a record identified by table and primary key with data listed in HTTP message body
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPut("{table}/{id}")]
        [EnableCors("_myAllowSpecificOrigins")]
        public ActionResult PutMetricDetails([FromRoute] string table, [FromRoute] string id, [FromBody] JsonElement item)
        {
            var result = metricRepository.UpdateTableItem(table, id, item);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// CREATE a record in a table with data listed in the HTTP message body
        /// </summary>
        /// <param name="table"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        [HttpPost("{table}")]
        [EnableCors("_myAllowSpecificOrigins")]
        public ActionResult PostMetricDetails([FromRoute] string table, [FromBody] JsonElement item)
        {
            var result = metricRepository.CreateTableItem(table, item);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        /// <summary>
        /// Delete a record from table based on primary key
        /// </summary>
        /// <param name="table"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{table}/{id}")]
        [EnableCors("_myAllowSpecificOrigins")]
        public ActionResult DeleteMetricItem([FromRoute] string table, [FromRoute] string id)
        {
            var result = metricRepository.DeleteTableItem(table, id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok();

        }
    }
}
