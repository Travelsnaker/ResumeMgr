using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResumeMgr.Models
{
    public class Resume
    {
        public string Id { get; set; }
        public DateTime UploadTime { get; set; }
        public string Content { get; set; }
    }
}