using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Final.Models
{
	public class Agent
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public ObjectId Id { get; set; }

		public string Name { get; set; }

		public string Address { get; set; }

		public string Description { get; set; }
	}
}
