using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FuelQueue.Models
{
    public class ObejectClass
    {
    }

    [BsonIgnoreExtraElements]
    public class FuelUserData
    {
        public ObjectId id { get; set; }

        public string VehicleNumber { get; set; }

        public string FuelType { get; set; }

        public string  FuelStation { get; set; }

        public  DateTime ArrivalTime { get; set; }

        public DateTime DepartTime { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class FuelInfo
    {
        public ObjectId id { get; set; }

        public string FuelStation { get; set; }

        public string FuelType { get; set; }

        public DateTime ArrivalTime { get; set; }

        public DateTime FinishTime { get; set; }
    }
}