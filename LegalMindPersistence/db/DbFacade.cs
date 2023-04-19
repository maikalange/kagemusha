using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LegalMindPersistence.db
{
    public class DbFacade
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
            if (documents.Count() > 0)
            {
                collection.InsertMany(documents);
            }
        }

        private static IMongoCollection<BsonDocument> GetCollection()
        {
            var dbConnection = Environment.GetEnvironmentVariable("mongodb.connection");
            
            if (dbClient==null&&collection==null)
            {
                dbClient = new MongoClient(dbConnection);
                var database = dbClient.GetDatabase("legalmind");
                collection = database.GetCollection<BsonDocument>("legislationgradeA");                
            }

            return collection;
        }
    }
}
