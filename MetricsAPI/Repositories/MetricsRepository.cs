﻿//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
//	Project:		Metrics
//	File Name:		MetricsRepository.cs
//	Description:	Provide methods to connect to a database and perform CRUD operations.
//	Course:			CSCI 4350-941 - Software Engineering II
//	Authors:		
//	Created:		Monday, October 12, 2020
//	Copyright:		
//
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using BikeShopAPI.Oracle;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Namespace for repository
/// </summary>
namespace BikeShopAPI.Repositories
{
    /// <summary>
    /// Class that provides methods to connect to an oracle database and perform CRUD operations. Inherits from the IMetricsRepository interface
    /// </summary>
    public class MetricsRepository : IMetricsRepository
    {
        
        IConfiguration configuration;
        private IDictionary<string, string> pkDict = new Dictionary<string, string>(); //Hold Primary key configurations for each table
        public MetricsRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
            pkDict.Add("customer", "customerid");
            pkDict.Add("inventory", "inventoryid");
            pkDict.Add("cart", "cartid");
        }
        /// <summary>
        /// Read a record from the database
        /// </summary>
        /// <param name="table"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public object GetTableItem(string table, string itemId)
        {
            object result = null;
            try
            {
                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    //Split Composite key strings on delimiter = '&' then format query according to key given
                    List<string> PKFields = new List<string>(pkDict[table.ToLower()].Split('&'));
                    List<string> CompositePK = new List<string>(itemId.Split('&'));
                    var query = "SELECT * FROM Metric." + table + " Where";
                    for (int i = 0; i < CompositePK.Count; i++)
                    {
                        query += " " + PKFields[i] + "=" + CompositePK[i];
                        if (i < CompositePK.Count - 1)
                        {
                            query += " and";
                        }
                    }
                    result = SqlMapper.Query(conn, query, commandType: CommandType.Text); //make query of database and store result
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        /// <summary>
        /// Read all records of a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public object GetTable(string table)
        {
            object result = null;
            try
            {
                var dyParam = new OracleDynamicParameters();
                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    var query = "SELECT * FROM Metric." + table;

                    result = SqlMapper.Query(conn, query, param: dyParam, commandType: CommandType.Text); //query the database and store result
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        /// <summary>
        /// Update fields of a record identified by primary key
        /// </summary>
        /// <param name="table">Tabke name</param>
        /// <param name="id">Primary key</param>
        /// <param name="item">Body of HTTP message</param>
        /// <returns></returns>
        public object UpdateTableItem(string table, string id, JsonElement item)
        {
            object result = null;
            Dictionary<string, string> values = Tools.JsonToDictionary(item);
            try
            {
                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    var query = "UPDATE Metric." + table + " SET"; //first part of query
                    foreach (KeyValuePair<string, string> entry in values) //input data to be updated
                    {
                        query += " " + entry.Key + "=" + entry.Value + ",";
                    }
                    query = query.Remove(query.Length - 1, 1);
                    query += " Where"; //start limiting by primary key
                    //Split Composite key strings on delimiter = '&' then format query according to key given
                    List<string> PKFields = new List<string>(pkDict[table.ToLower()].Split('&'));
                    List<string> CompositePK = new List<string>(id.Split('&'));
                    
                    for (int i = 0; i < CompositePK.Count; i++)
                    {
                        query += " " + PKFields[i] + "=" + CompositePK[i];
                        if (i < CompositePK.Count - 1)
                        {
                            query += " and";
                        }
                    }
                    result = SqlMapper.Query(conn, query, commandType: CommandType.Text);
                    SqlMapper.Query(conn, "COMMIT", commandType: CommandType.Text);
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        /// <summary>
        /// Create a record in the listed table using fields in the JsonElement
        /// </summary>
        /// <param name="table">Table Name</param>
        /// <param name="item">Body of HTTP message</param>
        /// <returns>Result of Query</returns>
        public object CreateTableItem(string table, JsonElement item)
        {
            object result = null;
            Dictionary<string, string> values = Tools.JsonToDictionary(item); //Use our converter to convert JSON to a dictionary while maintaining quotes in required areas
            try
            {
                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    var query = "INSERT INTO Metric." + table + " (";
                    string qValues = "VALUES (";
                    //Iterate over each values dictionary key:Value pair and add key to query while adding value to qValues
                    foreach (KeyValuePair<string, string> entry in values) //input data to be updated
                    {
                        query += " " + entry.Key + ",";
                        qValues += " " + entry.Value + ",";
                    }
                    query = query.Remove(query.Length - 1, 1); //remove last comma
                    query += ") ";
                    qValues = qValues.Remove(qValues.Length - 1, 1); //remove last comma
                    qValues += ")";
                    query += qValues;
                    result = SqlMapper.Query(conn, query, commandType: CommandType.Text);
                    
                    SqlMapper.Query(conn, "COMMIT", commandType: CommandType.Text);
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }
        /// <summary>
        /// Delete a record from the database
        /// </summary>
        /// <param name="table"></param>
        /// <param name="oldId"></param>
        /// <returns></returns>
        public object DeleteTableItem(string table, string oldId)
        {
            object result = null;
            try
            {
                var conn = this.GetConnection();
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }

                if (conn.State == ConnectionState.Open)
                {
                    List<string> PKFields = new List<string>(pkDict[table.ToLower()].Split('&'));
                    List<string> CompositePK = new List<string>(oldId.Split('&'));

                    var query = "DELETE FROM Metric." + table + " WHERE";
                    for (int i = 0; i < CompositePK.Count; i++)
                    {
                        query += " " + PKFields[i] + "=" + CompositePK[i];
                        if (i < CompositePK.Count - 1)
                        {
                            query += " and";
                        }
                    }
                    result = SqlMapper.Query(conn, query, commandType: CommandType.Text);
                    SqlMapper.Query(conn, "COMMIT", commandType: CommandType.Text);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;

        }
        /// <summary>
        /// Establish Connection to the database using data in appsettings.json
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            var connectionString = configuration.GetSection("ConnectionStrings").GetSection("MetricsConnection").Value;
            var conn = new OracleConnection(connectionString);
            return conn;
        }
    }
}

