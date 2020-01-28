using MongoDB.Bson;


namespace AuthService.DAL
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string name { get; set; }
        public string psw { get; set; }
        public BsonDocument Data { get; set; }
        public Role role { get; set; }
    }
}