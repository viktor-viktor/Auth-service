﻿using System;
using System.Linq;

using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthService.DAL
{
    //TODO: mongo should be configured with json file, 
    public class MongoDAL
    {
        public MongoDAL()
        {
            m_client = new MongoClient("mongodb://localhost:27017?connect=replicaSet");
            m_db = m_client.GetDatabase("TestC");

            if (!m_db.ListCollectionNames().ToList().Contains("Users"))
            {
                m_db.CreateCollection("Users");

                IMongoCollection<User> users = m_db.GetCollection<User>("Users");
                users.Indexes.CreateOne(Builders<User>.IndexKeys.Ascending(_ => _.name), new CreateIndexOptions { Unique = true });
            }
        }

        private MongoClient m_client = null;
        private IMongoDatabase m_db = null;
        
        public bool AddNewUer(string name, string psw, Role role = null)
        {
            if (role == null) role = Role.User;

            IMongoCollection<User> users = m_db.GetCollection<User>("Users");

            User nUser = new User() { name = name, psw = psw, role = role };

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

        public User GetUserData(string name, string psw)
        {
            IMongoCollection<User> users = m_db.GetCollection<User>("Users");

            var filter = Builders<User>.Filter.Eq("name", name) & Builders<User>.Filter.Eq("psw", psw);
            var value = users.Find<User>(filter);
            if (value.CountDocuments() == 0)
            {
                return null;
            }

            return value.First<User>();
        }
    }
}
