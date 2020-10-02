using Dapper;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace MetricsAPI.Services
{
    public class MetricsRepository : IMetricsRepository
    {

        IConfiguration configuration;
        private IDictionary<string, string> pkDict = new Dictionary<string, string>();
        public MetricsRepository(IConfiguration _configuration)
        {
            configuration = _configuration;
            pkDict.Add("bicycle", "serialnumber");
            pkDict.Add("bikeparts", "serialnumber");
            pkDict.Add("letterstyle", "letterstyle");
           
        }
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
                    var query = "SELECT * FROM BIKE_SHOP." + table + " WHERE " + pkDict[table.ToLower()] + " = " + itemId;

                    result = SqlMapper.Query(conn, query, commandType: CommandType.Text);

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

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
                    var query = "SELECT * FROM BIKE_SHOP." + table;

                    result = SqlMapper.Query(conn, query, param: dyParam, commandType: CommandType.Text);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        public IDbConnection GetConnection()
        {
            var connectionString = configuration.GetSection("ConnectionStrings").GetSection("BikeShopConnection").Value;
            var conn = new OracleConnection(connectionString);
            return conn;
        }
    }
}
