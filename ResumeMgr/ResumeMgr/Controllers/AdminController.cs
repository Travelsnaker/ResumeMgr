using MongoDB.Bson;
using ResumeMgr.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ResumeMgr.Controllers
{
    public class AdminController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage>GetAll()
        {
            Resumes resumes = new Resumes();
            IList<BsonDocument> documents = await resumes.getAll();
            if (documents.Count==0)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            else
                return Request.CreateResponse(HttpStatusCode.Found, documents);
        }        
    }
}
