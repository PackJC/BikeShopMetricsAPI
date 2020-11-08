//////////////////////////////////////////////////////////////////////////////////////////////////////////////////
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
using MetricsAPI.Oracle;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
/// <summary>
/// Namespace for repository
/// </summary>
namespace MetricsAPI.Repositories
{
    /// <summary>
    /// Class that provides methods to connect to an oracle database and perform CRUD operations. Inherits from the IMetricsRepository interface
    /// </summary>
    public class MetricsRepository : IMetricsRepository
    {
        
        IConfiguration configuration;
        private IDictionary<string, string> pkDict = new Dictionary<string, string>(); //Hold Primary key configurations for each table
        private IDictionary<string, string> AutoGenTables = new Dictionary<string, string>(); //list of tables that the post method shoult return Identity
        private IDictionary<string, string> CustomGenTables = new Dictionary<string, string>(); //list tables with keys that require external auto key generation
        public MetricsRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
            pkDict.Add("customer", "username");
            pkDict.Add("inventory", "inventoryid");
            pkDict.Add("cartitem", "username&itemid&itemtable");
            pkDict.Add("cartlog", "logid");
            pkDict.Add("wishlist", "listid");
            pkDict.Add("wishlistlog", "logid");
            pkDict.Add("wishlistitem", "listid&itemid&itemtable");
            pkDict.Add("itemviewlog", "logid");
            pkDict.Add("userloginlog", "logid");
            pkDict.Add("searchlog", "logid");

            AutoGenTables.Add("cartlog", "logid");
            AutoGenTables.Add("userloginlog", "logid");
            AutoGenTables.Add("wishlistlog", "logid");
            AutoGenTables.Add("searchlog", "logid");
            AutoGenTables.Add("itemviewlog", "logid");
            AutoGenTables.Add("wishlist", "listid");
            AutoGenTables.Add("checkoutlog", "logid");
            AutoGenTables.Add("roles", "id");
            AutoGenTables.Add("users", "id");
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
                        if (AutoGenTables.ContainsKey(table.ToLower())) //check if primary key value will be unique, if not then ignore and let auto gen take care of it
                        {
                            if (entry.Key.ToLower() == AutoGenTables[table.ToLower()])
                            { //this entry is for the primary key
                                if (entry.Value == null)
                                { //add it no problem, db can generate on null
                                    query += " " + entry.Key + ",";
                                    qValues += " " + entry.Value + ",";
                                }
                                else
                                { //check if value will be unique
                                    string pkquery = "SELECT * FROM " + table + " WHERE " + entry.Key + "=" + entry.Value;
                                    List<dynamic> pkresult = conn.Query(pkquery).ToList();
                                    if (pkresult.Count == 0)
                                    {//record with pk does not exist
                                        query += " " + entry.Key + ",";
                                        qValues += " " + entry.Value + ",";
                                    }
                                }
                            }
                            else
                            {
                                query += " " + entry.Key + ",";
                                qValues += " " + entry.Value + ",";
                            }
                        }
                        else
                        {
                            query += " " + entry.Key + ",";
                            qValues += " " + entry.Value + ",";
                        }
                    }
                    query = query.Remove(query.Length - 1, 1); //remove last comma
                    query += ") ";
                    qValues = qValues.Remove(qValues.Length - 1, 1); //remove last comma
                    qValues += ")";
                    query += qValues;
                    if (AutoGenTables.ContainsKey(table.ToLower()))
                    {
                        var param = new DynamicParameters();
                        param.Add(name: "logid", dbType: DbType.Int32, direction: ParameterDirection.Output);
                        query += @" returning " + AutoGenTables[table.ToLower()] + " into :logid";

                        var result2 = conn.Execute(query, param);
                        var Id = param.Get<int>("logid");
                        result = Id;
                    }
                    else if (CustomGenTables.ContainsKey(table.ToUpper()))
                    {
                        string idquery = "SELECT MAX(" + CustomGenTables[table.ToUpper()] + ") FROM " + table;
                        result = conn.QueryFirstOrDefault<string>(idquery); //get first value as string
                        string resultText = (result.ToString());
                        int identity = Tools.GetDirtyInt(resultText); //get the number associated with the id
                        identity++; //use next available identifier
                        //rebuild query
                        query = "INSERT INTO Metric." + table + " (";
                        qValues = "VALUES (";
                        bool ignoreAutoGen = false;
                        foreach (KeyValuePair<string, string> entry in values) //input data to be updated
                        {
                            if (CustomGenTables.ContainsKey(table.ToUpper())) //check if primary key value will be unique, if not then ignore and let auto gen take care of it
                            {
                                if (entry.Key.ToUpper() == CustomGenTables[table.ToUpper()])
                                { //this entry is for the primary key
                                    if (entry.Value != null)
                                    {
                                        string pkquery = "SELECT * FROM " + table + " WHERE " + entry.Key + "=" + entry.Value;
                                        List<dynamic> pkresult = conn.Query(pkquery).ToList();
                                        if (pkresult.Count == 0)
                                        {//record with pk does not exist
                                            query += " " + entry.Key + ",";
                                            qValues += " " + entry.Value + ",";
                                            ignoreAutoGen = true;
                                            result = entry.Value;
                                        }
                                    }
                                }
                                else
                                {
                                    query += " " + entry.Key + ",";
                                    qValues += " " + entry.Value + ",";
                                }
                            }
                            else
                            {
                                query += " " + entry.Key + ",";
                                qValues += " " + entry.Value + ",";
                            }
                        }
                        if (!ignoreAutoGen)
                        {
                            query += " " + CustomGenTables[table.ToUpper()];
                            qValues += " " + identity;
                        }
                        else
                        {
                            query = query.Remove(query.Length - 1, 1); //remove last comma
                            qValues = qValues.Remove(qValues.Length - 1, 1); //remove last comma
                        }
                        query += ") ";
                        qValues += ")";
                        query += qValues;
                        SqlMapper.Query(conn, query, commandType: CommandType.Text);
                    }
                    else
                    {
                        result = SqlMapper.Query(conn, query, commandType: CommandType.Text);
                    }
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
        /// Establish Connection to the database using data in appsettings.json
        /// </summary>
        /// <returns></returns>
        public IDbConnection GetConnection()
        {
            var connectionString = "data source=segfault.asuscomm.com:1522/orcl.asuscomm.com;password=segfault4350;user id=Metric;";
            var conn = new OracleConnection(connectionString);
            return conn;
        }
    }
}


