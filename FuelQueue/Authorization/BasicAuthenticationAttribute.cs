using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace FuelQueue.Authorization
{
    public class BasicAuthenticationAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {

            var descriptor = actionContext.ActionDescriptor;

            var controllerName = descriptor.ControllerDescriptor.ControllerName;



            if (actionContext.Request.Headers.Authorization != null)
            {
                var authToken = actionContext.Request.Headers.Authorization.Parameter;
                var decodeauthToken = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authToken));
                var arrUserNameandPassword = decodeauthToken.Split(':');
                Console.WriteLine(arrUserNameandPassword[0] + ":" + arrUserNameandPassword[1]);
                if (IsAuthorizedUser(arrUserNameandPassword[0], arrUserNameandPassword[1]))
                {
                    Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity(arrUserNameandPassword[0]), null);
                }
                else
                {
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
        }


        private static bool IsAuthorizedUser(string Username, string Password)
        {
            return Username == ConfigurationManager.AppSettings["ApiApplicationId"] && Password == ConfigurationManager.AppSettings["ApiApplicationSecretKey"];
        }
    }
}