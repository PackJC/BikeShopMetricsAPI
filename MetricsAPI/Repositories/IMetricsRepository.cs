//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:		Metrics
//	File Name:		IMetricsRepository.cs
//	Description:	Interface for database connection and access
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
using System.Text.Json;

namespace MetricsAPI.Repositories
{
    /// <summary>
    /// Interface for connecting to database
    /// </summary>
    public interface IMetricsRepository
    {
        object GetTable(string table);
        object GetTableItem(string table, string bicycleId);
        object UpdateTableItem(string table, string id, JsonElement item);
        object CreateTableItem(string table, JsonElement item);
        object DeleteTableItem(string table, string oldId);
    }
}
