using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;

namespace LegalMindPersistence.db
{
    internal class DbFacade
    {
        private static MongoClient dbClient;
        private static IMongoCollection<BsonDocument> collection;
        public static void Save(BsonDocument document)
        {
            IMongoCollection<BsonDocument> collection = GetCollection();

            collection.InsertOne(document);
        }
        public static void SaveMany(IEnumerable<BsonDocument> documents)
        {
            IMongoCollection<BsonDocument> collection = GetCollection();

            collection.InsertMany(documents);
        }

        private static IMongoCollection<BsonDocument> GetCollection()
        {

            if (dbClient==null&&collection==null)
            {
                dbClient = new MongoClient("mongodb+srv://nyirenda:76upJrcHFGAqwzlp@cluster0.rugxi.mongodb.net/test?authSource=admin&replicaSet=atlas-35ztlv-shard-0&readPreference=primary&appname=MongoDB%20Compass&ssl=true");
                var database = dbClient.GetDatabase("legalmind");
                collection = database.GetCollection<BsonDocument>("legislationgradeA");
                
            }

            return collection;
        }
    }
}
