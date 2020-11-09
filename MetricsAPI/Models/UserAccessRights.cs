using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetricsAPI.Models
{
    public class UserAccessRights
    {
        public DateTime Expiration { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public List<dynamic[]> AccessRights { get; set; }
        public UserAccessRights()
        {
            AccessRights = new List<dynamic[]>();
        }
    }
}
