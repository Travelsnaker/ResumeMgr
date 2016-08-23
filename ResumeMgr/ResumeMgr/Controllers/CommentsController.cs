using AuthenticationFilter;
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
    [IdentityBasicAuthentication]
    public class CommentsController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage>CreateComment([FromBody]Comment comment)
        {
            if (comment == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            Resumes resumes = new Resumes();
            if (await resumes.createComment(comment))
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);

        }
        [Authorize]
        [HttpGet]
        public async Task<HttpResponseMessage>GetComments(string Id)
        {
            Resumes resumes = new Resumes();
            IList<Comment> comments = await resumes.getCommentById(Id);
            if (comments == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            else
                return Request.CreateResponse(HttpStatusCode.Found, comments);
        }
        [Authorize]
        [HttpDelete]
        public async Task<HttpResponseMessage>DeleteComments(string id)
        {
            Resumes resumes = new Resumes();
            if (await resumes.deleteComments(id))
                return Request.CreateResponse(HttpStatusCode.OK);
            else
                return Request.CreateResponse(HttpStatusCode.NotFound);
        }
    }
}
