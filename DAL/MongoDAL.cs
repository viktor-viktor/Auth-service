using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Bson;
using MongoDB.Driver;

namespace CSharp.DAL
{
    class MongoDAL
    {
        private MongoClient m_client = null;
        private IMongoDatabase m_db = null;
        
        private class User
        {
            public User(string _name, string _psw){ name = _name; psw = _psw; }
            public User() { }
            public ObjectId Id;
            public string name;
            public string psw;
        }

        public void Init()
        {
            m_client = new MongoClient("mongodb://localhost:27017?connect=replicaSet");
            m_db = m_client.GetDatabase("TestC");

            if (!m_db.ListCollectionNames().ToList().Contains("Users"))
            {
                m_db.CreateCollection("Users");

                IMongoCollection<User> users = m_db.GetCollection<User>("Users");
                users.Indexes.CreateOne(Builders<User>.IndexKeys.Ascending(_=>_.name), new CreateIndexOptions { Unique = true });
            }
        }

        public bool AddNewUer(string name, string psw)
        {
            IMongoCollection<User> users = m_db.GetCollection<User>("Users");

            User nUser = new User();
            nUser.name = name;
            nUser.psw = psw;

            try
            {
                users.InsertOne(nUser);
            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool RemoveUser(string name, string psw)
        {
            IMongoCollection<BsonDocument> users = m_db.GetCollection<BsonDocument>("Users");

            DeleteResult result = users.DeleteOne(new BsonDocument() { { "name", name }, { "psw", psw } } );

            return true;
        }

        public string GetUserData(string name, string psw)
        {
            IMongoCollection<BsonDocument> users = m_db.GetCollection<BsonDocument>("Users");

            var value = users.Find<BsonDocument>(new BsonDocument() { { "name", name }, { "psw", psw } });
            if (value.CountDocuments() == 0)
            {
                return null;
            }

            return name;
        }
    }
}
