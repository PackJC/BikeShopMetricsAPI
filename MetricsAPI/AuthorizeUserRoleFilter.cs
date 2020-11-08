using MetricsAPI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
//using System.Web.Http.Filters;
using System.Diagnostics;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Filters;

namespace MetricsAPI
{
    public class AuthorizeUserRoleFilter : ActionFilterAttribute
    {
        public HttpContext httpContextBase { get; set; }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            httpContextBase = filterContext.HttpContext;
            //public override void OnActionExecuting(HttpActionContext httpContextBase)

            //get user access rights via active users
            string queryURI = httpContextBase.Request.GetDisplayUrl();
            string method = httpContextBase.Request.Method.ToString();
            string accessToken = httpContextBase.Request.Headers[HeaderNames.Authorization];
            if (accessToken == null)
            {
                var response = new HttpResponseMessage
                {
                    Content =
                            new StringContent("This token is not valid, please refresh token or obtain valid token!"),
                    StatusCode = HttpStatusCode.Unauthorized
                };
                filterContext.Result = ((ControllerBase)filterContext.Controller).Unauthorized();
                return;
            }
            accessToken = accessToken.Remove(0, 7);//remove "Bearer " from token;
            List<dynamic[]> accessRights = new List<dynamic[]>();
            foreach (UserAccessRights user in Program.ActiveUsers)
            {
                if (user.Token == accessToken)
                {
                    accessRights = user.AccessRights;
                    break;
                }
            }
            //add code to break if no access rights found
            //find out what table they are trying to access and what type of access Create/read/update/delete
            Dictionary<string, int> methodIndex = new Dictionary<string, int>();
            methodIndex.Add("POST", 1);
            methodIndex.Add("GET", 2);
            methodIndex.Add("PUT", 3);
            methodIndex.Add("DELETE", 4);
            string locate = @"metrics/";
            int location = queryURI.ToLower().IndexOf(locate) + 8;
            string cut = queryURI.Substring(location, queryURI.Length - location);//get section with table and  an optional key
            string table = cut.Split('/')[0].ToLower(); //first thing will be table
            bool canAccess = false;
            //check accessRights to see if user can continue
            foreach (dynamic[] rights in accessRights)
            {
                if ((string)rights[0] == table && (int)rights[methodIndex[method]] == 1)
                {
                    canAccess = true;
                    break;
                }
            }

            if (!canAccess)
            {
                filterContext.Result = ((ControllerBase)filterContext.Controller).Unauthorized();
                return;
            }
        }

    }
}
