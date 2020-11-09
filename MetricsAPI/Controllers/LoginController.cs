using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MetricsAPI.Models;
using System.Xml.Linq;
using System.Collections.Generic;
using Dapper;
using MetricsAPI.Oracle;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using MetricsAPI.Repositories;
using Newtonsoft.Json;

namespace MetricsAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _config;

        public LoginController(IConfiguration config)
        {
            _config = config;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] UserModel login)
        {
            IActionResult response = Unauthorized();
            var user = AuthenticateUser(login);

            if (user != null)
            {
                CleanExpiredTokens();
                var tokenString = GenerateJSONWebToken(user);
                UserAccessRights newlogin= new UserAccessRights();
                newlogin.Expiration = DateTime.Now.AddMinutes(120); //token expires in 120 minutes
                newlogin.Token = tokenString;
                newlogin.Username = user.Username;
                //get access rights from db and add to newlogin
                IEnumerable<IDictionary<string, object>> result = (IEnumerable<IDictionary<string, object>>)GetTable("USERROLES");//get all records listing which roleids a user has
                //for each role a user has add the access rights to newlogin
                List<int> roles = new List<int>();
                foreach (IDictionary<string, object> role in result)
                {
                    if (Decimal.ToInt32((Decimal)role["USERID"]) == user.UserId) //add role if it has user
                    {
                        roles.Add(Decimal.ToInt32((Decimal)role["ROLEID"]));
                    }
                }
                result = (IEnumerable<IDictionary<string, object>>)GetTable("ROLERIGHTS");//get all rights by roleid
                foreach (IDictionary<string,object> record in result)
                {
                    if (roles.Contains(Decimal.ToInt32((Decimal)record["ROLEID"])))
                    {
                        dynamic[] rights = new dynamic[5];
                        rights[0] = record["TABLENAME"];
                        rights[1] = record["CREATEACCESS"];
                        rights[2] = record["READACCESS"];
                        rights[3] = record["UPDATEACCESS"];
                        rights[4] = record["DELETEACCESS"];
                        newlogin.AccessRights.Add(rights);
                    }
                }
                
                Program.ActiveUsers.Add(newlogin); //add new user to active users

                response = Ok(new { token = tokenString });
            }

            return response;
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

        public IDbConnection GetConnection()
        {
            var connectionString = "data source=segfault.asuscomm.com:1522/orcl.asuscomm.com;password=segfault4350;user id=Metric;";
            var conn = new OracleConnection(connectionString);
            return conn;
        }

        private string GenerateJSONWebToken(UserModel userInfo)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);
            //save username to a global dictionary object with the token as the key

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private UserModel AuthenticateUser(UserModel login)
        {
            UserModel user = null;
            //get users from table
            IEnumerable<IDictionary<string, object>> result = (IEnumerable<IDictionary<string, object>>)GetTable("USERS");
            //check if username exists in table
            bool exists = false;
            string passwordhash = "";
            int id = 0;
            foreach (IDictionary<string,object> userdata in result)
            {
                if ((string)userdata["USERNAME"] == login.Username)
                {
                    exists = true;
                    passwordhash = (string)userdata["PASSWORDHASH"];
                    id = Decimal.ToInt32((Decimal)userdata["ID"]);
                    break;
                }
            }
            if (exists)
            {
                if (login.PasswordHash == passwordhash)
                {
                    user = new UserModel { Username = login.Username, UserId = id };
                }
            }
            return user;
        }

        private static void CleanExpiredTokens()
        {
            
            DateTime CurrentTime = DateTime.Now;
            List<int> Expired = new List<int>();
            for (int i = 0; i < Program.ActiveUsers.Count; i++)
            {
                if (CurrentTime > Program.ActiveUsers[i].Expiration)
                { //token has expired
                    Expired.Add(i);
                }
            }
            for (int i = Expired.Count - 1; i >= 0; i--)
            {
                Program.ActiveUsers.RemoveAt(Expired[i]);
            }
        }
    }
}
