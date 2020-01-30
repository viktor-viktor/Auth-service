using System;
using System.Linq;
using System.Text.Json;

using MongoDB.Bson;
using MongoDB.Driver;

namespace AuthService.DAL
{
    public class MongoDAL
    {
        public MongoDAL(string connectionString, string dbName)
        {
            m_client = new MongoClient(connectionString);
            m_db = m_client.GetDatabase(dbName);

            if (!m_db.ListCollectionNames().ToList().Contains("Users"))
            {
                m_db.CreateCollection("Users");

                IMongoCollection<User> users = m_db.GetCollection<User>("Users");
                users.Indexes.CreateOne(Builders<User>.IndexKeys.Ascending(_ => _.name), new CreateIndexOptions { Unique = true });
            }
        }

        private MongoClient m_client = null;
        private IMongoDatabase m_db = null;
        
        public User AddNewUer(string name, string psw, JsonElement data, Role role = null)
        {
            if (role == null) role = Role.User;

            IMongoCollection<User> users = m_db.GetCollection<User>("Users");
            User nUser = new User() { name = name, psw = psw, role = role , Data = BsonDocument.Parse(data.ToString())};

            try
            {
                users.InsertOne(nUser);
            }
            catch (Exception e)
            {
                return null;
            }

            return nUser;
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
