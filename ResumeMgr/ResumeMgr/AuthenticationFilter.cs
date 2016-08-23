////////////////////////////////////////////////////////////////////////////
// AuthenticationFilter.cs - Defines authentication filter that performs  //
//                           authenticate process                         //
// Ver 1.0                                                                //
// Application: CSE775   Probing Example                                  //
// Language:    C#, ver 6.0, Visual Studio 2015                           //
// Platform:    lenovo Y470, Core-i3, Windows 7                           //
// Author:      Wei Sun, Syracuse University                              //
//              wsun13@syr.edu                                            //
////////////////////////////////////////////////////////////////////////////
/*
*  Package Operation:
*  ===============================
*  This package provodes two classes:
*
*  class AddChallengeOnUnauthorizedResult:
*  =================================
*  This class adds challenge on a unauthorized IHttpActionResult. It creates a
*  new HttpActionResult which combines the old HttpActionResult and challenge. 
*  If the request is unauthenticated, it will add Www-Authenticate in response 
*  http header. 
*
*  class IdentityBasicAuthentication:
*  =====================================
*  This class defines authentication filter. It derives from class Attribute and
*  class IAuthenticationFilter and overrides methods AuthenticateAsync, AllowMultiple, 
*  ChallengeAsync.
*
*  Public Interface:
*  ======================
*  public Task AuthenticateAsync(HttpAuthenticationContext context,CancellationToken cancellationToken);
*  public bool AllowMultiple{ get { return false; }}
*  public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken);
*  
*  Maintance History:
*  ===================
*  ver 1.0 : 4/5/2016
*  -first release
*/

using ResumeMgr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.Results;

namespace AuthenticationFilter
{
    

    // Custome authentication filter 
    public class IdentityBasicAuthentication : Attribute,IAuthenticationFilter
    {
        public string Realm { get; set; }

        //<----defines the authenticate logic---->
        public async Task AuthenticateAsync(HttpAuthenticationContext context,CancellationToken cancellationToken)
        {           
            HttpRequestMessage request = context.Request;
            AuthenticationHeaderValue authentication = request.Headers.Authorization;
            if(request.Headers.Authorization!=null&&request.Headers.Authorization.Scheme.Equals("basic",StringComparison.OrdinalIgnoreCase))
            {
                string enUsernameAndPassword = authentication.Parameter;
                if(enUsernameAndPassword!=null)
                {
                    try
                    {
                        // Convert 64-base encoding credential to binary
                        Encoding encoding = Encoding.GetEncoding("ISO-8859-1");
                        string UsernameAndPassword;
                        UsernameAndPassword = encoding.GetString(Convert.FromBase64String(enUsernameAndPassword));
                        int seperator = UsernameAndPassword.IndexOf(':');
                        string username = UsernameAndPassword.Substring(0, seperator);
                        string password = UsernameAndPassword.Substring(seperator + 1);
                        Resumes resumes = new Resumes();
                        // if username is Tom, password is 123, authenticate. Otherwise, unauthenticate.
                        if (await resumes.checkIndent(username,password)!=null)
                        {
                            var claims = new List<Claim>()
                            {
                                new Claim(ClaimTypes.Name,username)
                            };
                            var id = new ClaimsIdentity(claims, "Basic");
                            var principal = new ClaimsPrincipal(new[] { id });
                            context.Principal = principal;
                        }
                    }
                    catch (FormatException)
                    {
                        HttpContext.Current.Response.StatusCode = 401;
                    }
                }              
            }            
            else
            {
                context.ErrorResult = new UnauthorizedResult(new AuthenticationHeaderValue[0], context.Request);
            }           
        }
        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            Challenge(context);
            return Task.FromResult(0);
        }

        //<----determines whether filters allow multiple authentications---->
        public bool AllowMultiple
        {
            get { return false; }
        }
        //<----adds an authentication challenge to HTTP responds, if needed---> 
        private void Challenge(HttpAuthenticationChallengeContext context)
        {
            string parameter;

            if (String.IsNullOrEmpty(Realm))
            {
                parameter = null;
            }
            else
            {
                // A correct implementation should verify that Realm does not contain a quote character unless properly
                // escaped (precededed by a backslash that is not itself escaped).
                parameter = "realm=\"" + Realm + "\"";
            }

            context.ChallengeWith("Basic", parameter);
        }

    }
    public static class HttpAuthenticationChallengeContextExtensions
    {
        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme));
        }

        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, string scheme, string parameter)
        {
            ChallengeWith(context, new AuthenticationHeaderValue(scheme, parameter));
        }

        public static void ChallengeWith(this HttpAuthenticationChallengeContext context, AuthenticationHeaderValue challenge)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
        }
    }
    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }
        public AuthenticationHeaderValue Challenge { get; private set; }
        //<----InnerResult represents for the old IHttpActionResult---->
        public IHttpActionResult InnerResult { get; private set; }
        //<---- If the request is unauthorized, it will add WWW-Authenticate in http header---->
        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
           
                HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Only add one challenge per authentication scheme.
                    if (!response.Headers.WwwAuthenticate.Any((h) => h.Scheme == Challenge.Scheme))
                    {
                        response.Headers.WwwAuthenticate.Add(Challenge);                
                    }
                }
                return response;           
        }
    }
}