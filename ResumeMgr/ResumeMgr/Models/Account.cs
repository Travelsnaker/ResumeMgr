using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResumeMgr.Models
{
    public class Account
    {
        public string Id{get;set;}
        public string AccountName { get; set; }
    }
}