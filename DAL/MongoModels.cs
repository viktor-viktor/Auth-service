using MongoDB.Bson;

using System.Text.Json;

namespace AuthService.DAL
{
    public class User
    {
        public string GetDataString() 
        {
            return JsonSerializer.Deserialize<string>(Data.ToString());
        }
        public ObjectId Id { get; set; }
        public string name { get; set; }
        public string psw { get; set; }
        public BsonDocument Data { get; set; }
        public Role role { get; set; }
    }
}