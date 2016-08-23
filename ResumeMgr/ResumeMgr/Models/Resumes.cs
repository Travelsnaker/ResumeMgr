///////////////////////////////////////////////////////////////////
// Resumes.cs - Connect to the mongo database and provide methods//
//              get access to the database.                      //
// Ver 1.0                                                       //
// Application: CSE775   Probing Example                         //
// Language:    C#, ver 6.0, Visual Studio 2015                  //
// Platform:    lenovo Y470, Core-i3, Windows 7                  //
// Author:      Wei Sun, Syracuse University                     //
//              wsun13@syr.edu                                   //
///////////////////////////////////////////////////////////////////
/*
* Package Operations:
* ===================
* This package is responsible for connecting to the mongo database and
* provides methods to get access to the database.
* 
* Public Interface:
* ===================
* public Resumes();
* public IList<Resume> GetAll();
* public Resume Get(ObjectId id);
* public ObjectId Insert(Resume resume);
* public ObjectId Update(ObjectId id,Resume resume);
* public bool Delete(ObjectId id);
*
* Maintance:
* Required Files:resume.cs, Comment.cs, Person.cs
*
* Mongodb driver install process:
* Tools->NuGet Package Manager->Package Manager Console> install-Package mongocsharpdriver 
*
*  Maintance History:
*  ===================
*  ver 1.0 : 4/5/2016
*  -first release
*/
using MongoDB.Driver;
using System.Linq;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Web.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using System;
using MongoDB.Driver.Linq;

namespace ResumeMgr.Models
{
    public class Resumes
    {
        private Process mongod;
        private IMongoDatabase database;
        //<---- shut down the mongodb server---->
        ~Resumes()
        {
            mongod.Kill();
        }
        //<---- Set up mongo db connection string, start mongodb server---->
        public Resumes()
        { 
            // set up the connection string         
            var connectionstring = "mongodb://localhost";
            var client = new MongoClient(connectionstring);
            var path = System.Web.HttpContext.Current.Server.MapPath("~");
            // create a new process to run mongod server           
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = path+"mongod.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            // set up the path for data storage. In this case, ~\ResumeMgr\ResumeMgr\data\db
            start.Arguments = "--dbpath "+path+"data\\db";
            mongod = Process.Start(start);          
            // Open the database. If it doesn't exist, create it first.
            database = client.GetDatabase("Resume");
        }

        public async Task<string>checkIndent(string account,string password)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("Account", account);
            var result = await collection.Find(filter).ToListAsync();
            if (result.Count != 1)
                return null;
            if (result[0]["Password"] != password)
                return null;
            return result[0]["_id"].ToString();
        }

        public async Task<ObjectId> Insert(BsonDocument document)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            await collection.InsertOneAsync(document);
            return (ObjectId)document["_id"]; 
        }

        public async Task<bool>accountExist(string account)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("Account", account);
            var result = await collection.Find(filter).ToListAsync();
            if (result.Count > 0)
                return true;
            return false;
        }
        public async Task<bool>find(ObjectId id)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var result = await collection.Find(filter).ToListAsync();
            if (result.Count!= 1)
                return false;
            else
                return true;
        }

        public async Task<bool>createResume(Resume resume)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");           
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(resume.Id));
            var update = Builders<BsonDocument>.Update.Set("Resumes.UploadTime", resume.UploadTime)
                     .Set("Resumes.Content", resume.Content);
            var result = await collection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged)
                return false;
            if (result.MatchedCount==0)
                return false;
            return true;          
        } 

        public async Task<bool>createComment(Comment comment)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(comment.Id));
            var result=await collection.Find(filter).ToListAsync();
            if (result.Count!=1)
                return false;           
            if (!result[0].Contains("Comments"))
                result[0]["Comments"] = new BsonArray();
            BsonDocument field = new BsonDocument();
            field["UploadTime"] = comment.UploadTime;
            field["Content"] = comment.Content;
            result[0]["Comments"].AsBsonArray.Add(field);           
            await collection.ReplaceOneAsync(filter,result[0]);
            return true;
        }
        //<---- Get all the document in collection "resumes"---->
        public async Task<IList<BsonDocument>>getAll()
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Ne("_id", "null");
            IList<BsonDocument> documents = await collection.Find(filter).ToListAsync();
            return documents;
        }
        public async Task<IList<Account>>getAllAccounts()
        {
            IList<Account> accounts = new List<Account>();
            var collection = database.GetCollection<BsonDocument>("resumes");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Ne("Resumes", "null");
            var result = await collection.Find(filter).ToListAsync();
            if (result.Count == 0)
                return null;
            foreach(var document in result)
            {
                Account account = new Account();
                account.Id = document["_id"].ToString();
                account.AccountName = document["Account"].ToString();
                accounts.Add(account);
            }
            return accounts;
        }
        //<----Get resume by id---->
        public async Task<Resume>getResumeById(string Id)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("_id", ObjectId.Parse(Id));
            var documents = await collection.Find(filter).ToListAsync();
            if (documents.Count != 1)
                return null;
            if (!documents[0].Contains("Resumes"))
                return null;
            Resume resume = new Resume();
            BsonDocument doc = documents[0]["Resumes"].ToBsonDocument();
            resume.Id = Id;
            resume.UploadTime = (DateTime)doc["UploadTime"];
            resume.Content = doc["Content"].ToString();
            return resume;
        }  
        
        //<---Get comments by id---->
        public async Task<IList<Comment>>getCommentById(string Id)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("_id", ObjectId.Parse(Id))&builder.Ne("Comments","null");
            var documents = await collection.Find(filter).ToListAsync();
            if (documents.Count !=1)
                return null;
            if (!documents[0].Contains("Comments"))
                return null;
            IList<Comment> comments = new List<Comment>();
            var docs = documents[0]["Comments"].AsBsonArray;
            foreach(var comment in docs)
            {
                Comment cmt = new Comment();
                cmt.Id = Id;
                cmt.UploadTime = (DateTime)comment["UploadTime"];
                cmt.Content = comment["Content"].ToString();
                comments.Add(cmt);
            }
            return comments;
        }   
        
        //<----delete resume---->
        public async Task<bool>deleteResumeById(string id)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));
            var update = Builders<BsonDocument>.Update.Unset("Resumes");
            var result = await collection.UpdateOneAsync(filter, update);
            if (!result.IsAcknowledged)
                return false;
            if (result.MatchedCount == 0)
                return false;
            return true;
        }
        
        //<----delete comments---->
        public async Task<bool>deleteComments(string id)
        {
            var collection = database.GetCollection<BsonDocument>("resumes");
            var filter = Builders<BsonDocument>.Filter.Eq("id", ObjectId.Parse(id));
            var update = Builders<BsonDocument>.Update.PullFilter("Comments",Builders<BsonDocument>.Filter.AnyNe("Content","null"));
            var result = await collection.FindOneAndUpdateAsync(filter, update);         
            return true;
        }       
    }
}