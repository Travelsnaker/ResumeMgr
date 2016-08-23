using AuthenticationFilter;
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
    public class PersonsController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage>CreateAccount([FromBody]Person person)
        {
            if (person == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            BsonDocument document = new BsonDocument();
            Resumes resumes = new Resumes();
            if (await resumes.accountExist(person.Account))
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            document["Account"] = person.Account;
            document["Name"] = person.Name;
            document["Email"] = person.Email;
            document["Password"] = person.Password;            
            ObjectId Id = await resumes.Insert(document);
            return Request.CreateResponse(HttpStatusCode.Created, Id);
        }
        
        
        [HttpPut]
        public async Task<HttpResponseMessage> LoginAccount([FromBody]Credent credential)
        {
            Resumes resumes = new Resumes();
            string id = await resumes.checkIndent(credential.Account, credential.Password);
            if (id == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            return Request.CreateResponse(HttpStatusCode.Found,id);
        }
    }
}
