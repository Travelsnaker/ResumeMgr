///////////////////////////////////////////////////////////////////
// ResumesController.cs - Handle Http request, perform get, put  //
//                        Post,delete operation                  //
// Ver 1.0                                                       //
// Application: CSE775   Probing Example                         //
// Language:    C#, ver 6.0, Visual Studio 2015                  //
// Platform:    lenovo Y470, Core-i3, Windows 7                  //
// Author:      Wei Sun, Syracuse University                     //
//              wsun13@syr.edu                                   //
///////////////////////////////////////////////////////////////////
/*
*  Package Operation:
*  ===================
*  This package handles HTTP request. It authenticates each request and refuses 
*  any anonymous request. Any client, no matter whether authentical or not, can
*  get access to Get(), Get(string id), Post([FromBody]Resume resume) method.
*  Only authenticate client, in this case, Authorization:Basic Tom:123, can access
*  Put(string id,[FromBody]Resume resume) and Delete(string id).
*
*  Public Interface:
*  ===================
*  public HttpResponseMessage Get();
*  public HttpResponseMessage Get(string id);
*  public HttpResponseMessage Post([FromBody]Resume resume);
*  public HttpResponseMessage Put(string id,[FromBody]Resume resume);
*  HttpResponseMessage Delete(string id);
*
*  Maintance:
*  ===================
*  Required Files: AuthenticationFilter.cs, Resume.cs, Resumes.cs
*
*  Maintance History:
*  ===================
*  ver 1.0 : 4/5/2016
*  -first release
*
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using MongoDB.Driver;
using System.Web.Http;
using System.Net.Http;
using MongoDB.Bson;
using AuthenticationFilter;
using Newtonsoft.Json.Linq;
using ResumeMgr.Models;
using System.Net;
using System.Threading.Tasks;

namespace ResumeMgr.Controllers
{
    [IdentityBasicAuthentication]    
    public class ResumesController : ApiController
    {
        [Authorize]
        [HttpPut]       
        public async Task<HttpResponseMessage> CreateResume([FromBody]Resume resume)
        {
            if (resume == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            Resumes resumes = new Resumes();
            int last = resume.Id.Length - 1;
            string tmp=resume.Id.Remove(last);
            resume.Id=tmp.Remove(0, 1);
            string id = resume.Id;
            if (await resumes.createResume(resume))
            {
                return Request.CreateResponse(HttpStatusCode.OK);              
            }
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        
        [HttpGet]
        public async Task<HttpResponseMessage>GetAll()
        {          
            Resumes resumes = new Resumes();
            IList<Account> accounts = await resumes.getAllAccounts();
            if (accounts != null)
            {
                return Request.CreateResponse(HttpStatusCode.Found,accounts);
            }
                
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        } 
        
        //Get api/Resumes/id
        [HttpGet]
        public async Task<HttpResponseMessage>GetResume(string id)
        {
            Resumes resumes = new Resumes();
            Resume resume = await resumes.getResumeById(id);
            if (resume == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            else
                return Request.CreateResponse(HttpStatusCode.Found, resume);
        } 
        
        [HttpDelete]
        public async Task<HttpResponseMessage>DeleteResume(string id)
        {
            Resumes resumes = new Resumes();
            if (await resumes.deleteResumeById(id))
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }     
    }
}