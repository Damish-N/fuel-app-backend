using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FuelQueue.Authorization;
using FuelQueue.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FuelQueue.Controllers
{
    [Route("api/Fuel/{action}")]
    [BasicAuthentication]
    public class FuelController : ApiController
    {
        private MongoClient client;
        private IMongoDatabase database;

        public FuelController()
        {
            try
            {
                var settings = MongoClientSettings.FromConnectionString("mongodb+srv://fuelapp_2022:FuelApp2022@cluster0.y9mkusa.mongodb.net/?retryWrites=true&w=majority");
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                client = new MongoClient(settings);
                database = client.GetDatabase("FuelSystemDB");
            }
            catch (Exception)
            {

                throw;
            }

        }

        [HttpGet]
        //[Route("api/Fuel/GetAllFuelStations")]
        public IHttpActionResult GetAllFuelStations(string FuelType)
        {
            var status = "Fail";
            List<FuelInfo> collectedInfo = new List<FuelInfo>();


            try
            {
                status = "Success";
                var collection = database.GetCollection<FuelInfo>("FuelInfo");
                var collected = collection.Find(x =>  x.FuelType == FuelType && x.FinishTime == Convert.ToDateTime("0001-01-01T00:00:00.000+00:00")).ToList();
            return Ok(new { status = status, message = "Information Found", data = collected });

                
            }
            catch (Exception)
            {


            }
            return Ok(new { status = status, message = "Information Found", data = collectedInfo });

        }

        //User
        //Get Fuel Queue
        [HttpGet]
        //[Route("api/Fuel/GetFuelQueue")]
        public IHttpActionResult GetFuelQueue(string fuelStation,string fuelType)
        {
            var status = "Fail";
            long QueueCount = 0;
            try
            {
                var collection = database.GetCollection<FuelUserData>("FuelUserData");
                var collectedInfo = collection.Find(x => x.FuelStation == fuelStation && x.FuelType == fuelType && x.DepartTime == Convert.ToDateTime("0001-01-01T00:00:00.000+00:00"));
                QueueCount = collectedInfo.Count();
                status = "Success";
            }
            catch (Exception)
            {


            }
            return Ok(new { status = status, message = "Information Found", data = QueueCount });
        }

        //Get Fuel Stats
        [HttpGet]
        //[Route("api/Fuel/GetFuelStatus")]
        public IHttpActionResult GetFuelStatus(string FuelStation,string FuelType)
        {
            var status = "Fail";
            string message = string.Empty;
            FuelInfo retrunObj = new FuelInfo();
            try
            {
                var collection = database.GetCollection<FuelInfo>("FuelInfo");
                retrunObj = collection.Find(x => x.FuelStation == FuelStation && x.FuelType == FuelType).FirstOrDefault();
                status = "Success";
                message = "Information Found";
            }
            catch (Exception ex)
            {
                message = ex.ToString();

            }
            return Ok(new { status = status, message = message, data = retrunObj });
        }
        //Get Queue Waiting Time
        [HttpGet]
        //[Route("api/Fuel/GetQueueWaitingTime")]
        public IHttpActionResult GetQueueWaitingTime(string vehicleNumber)
        {
            var status = "Fail";
            string message = string.Empty;
            TimeSpan timeSpan = new TimeSpan();
            try
            {
                var collection = database.GetCollection<FuelUserData>("FuelUserData");
                var collectedInfo = collection.Find(x => x.VehicleNumber == vehicleNumber).ToList();
                var lastRecord = collectedInfo[collectedInfo.Count() - 1];
                DateTime lastTime = System.DateTime.Now.AddHours(5).AddMinutes(30);
                status = "Success";
                timeSpan = lastRecord.DepartTime - lastRecord.ArrivalTime;
                if (timeSpan.TotalSeconds > 0)
                {
                    message = "Vehicle is not in the queue";
                }
                else
                {
                    if(lastRecord.FuelStation == null)
                    {
                        message = "Vehicle is not in the queue";
                    }
                    else
                    {
                        message = "Vehicle is in the queue on station:" + lastRecord.FuelStation;
                        timeSpan = lastTime - lastRecord.ArrivalTime;
                    }
                    
                }


            }
            catch (Exception ex)
            {
                message = ex.ToString();

            }
            return Ok(new { status = status, message = message, data = timeSpan });
        }

        //User
        //Post Arrival Time
        [HttpPost]
        //[Route("api/Fuel/UpdateUserArrivalTime")]
        public IHttpActionResult UpdateUserArrivalTime(FuelUserData fuelUserData)
        {
            string message = string.Empty;
            var status = "Fail";
            try
            {

               
                var collection = database.GetCollection<FuelUserData>("FuelUserData");
                var retrunObj = collection.Find(x => x.VehicleNumber == fuelUserData.VehicleNumber && x.DepartTime == Convert.ToDateTime("0001-01-01T00:00:00.000+00:00")&& x.ArrivalTime != Convert.ToDateTime("0001-01-01T00:00:00.000+00:00")).FirstOrDefault();
                if (retrunObj != null)
                {
                    message = "Depart the previous visit before enter the next visit";
                }
                else
                {
                    var coll = collection.Find(x => x.VehicleNumber == fuelUserData.VehicleNumber).FirstOrDefault();
                    //collection.InsertOne(fuelUserData);
                    if (coll != null)
                    {
                        
                        fuelUserData.ArrivalTime = System.DateTime.Now.AddHours(5).AddMinutes(30);
                        collection.FindOneAndUpdate(x => x.VehicleNumber == fuelUserData.VehicleNumber, new UpdateDefinitionBuilder<FuelUserData>().Set(x => x.ArrivalTime, fuelUserData.ArrivalTime));
                        collection.FindOneAndUpdate(x => x.VehicleNumber == fuelUserData.VehicleNumber, new UpdateDefinitionBuilder<FuelUserData>().Set(x => x.DepartTime, Convert.ToDateTime("0001-01-01T00:00:00.000+00:00")));
                        collection.FindOneAndUpdate(x => x.VehicleNumber == fuelUserData.VehicleNumber, new UpdateDefinitionBuilder<FuelUserData>().Set(x => x.FuelStation,fuelUserData.FuelStation));

                        message = "Reocrd Updated";
                    }
                    else
                    {
                        fuelUserData.ArrivalTime = Convert.ToDateTime("0001-01-01T00:00:00.000+00:00");
                        collection.InsertOne(fuelUserData);
                        message = "Inserted new Record";
                    }
                }

                status = "Success";
                
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }

            return Ok(new { status = status, message = message });
        }

        //Post Depart TIme
        [HttpPost]
        //[Route("api/Fuel/UpdateUserDepartTime")]
        public IHttpActionResult UpdateUserDepartTime(FuelUserData fuelUserData)
        {
            var status = "Fail";
            string message = string.Empty;
            try
            {
                var collection = database.GetCollection<FuelUserData>("FuelUserData");
                collection.FindOneAndUpdate(x => x.VehicleNumber == fuelUserData.VehicleNumber && x.DepartTime == Convert.ToDateTime("0001-01-01T00:00:00.000+00:00"), new UpdateDefinitionBuilder<FuelUserData>().Set(x => x.DepartTime, System.DateTime.Now.AddHours(5).AddMinutes(30)));
                status = "Success";
                message = "Record Updated";
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }

            return Ok(new { status = status, message = message });
        }


        //Station Owner
        //Fuel Arrival Time
        [HttpPost]
        //[Route("api/Fuel/UpdateFuelArriveTime")]
        public IHttpActionResult UpdateFuelArriveTime(FuelInfo fuelInfo)
        {
            var status = "Fail";
            string message = string.Empty;
            try
            {
                var collection = database.GetCollection<FuelInfo>("FuelInfo");
                var retrunObj = collection.Find(x => x.FuelStation == fuelInfo.FuelStation && x.FuelType == fuelInfo.FuelType).FirstOrDefault();
                if (retrunObj == null)
                {
                    collection.InsertOne(fuelInfo);
                }
                else
                {
                    collection.FindOneAndUpdate(x => x.FuelStation == fuelInfo.FuelStation && x.FuelType == fuelInfo.FuelType, new UpdateDefinitionBuilder<FuelInfo>().Set(x => x.ArrivalTime, fuelInfo.ArrivalTime));
                    collection.FindOneAndUpdate(x => x.FuelStation == fuelInfo.FuelStation && x.FuelType == fuelInfo.FuelType, new UpdateDefinitionBuilder<FuelInfo>().Set(x => x.FinishTime, Convert.ToDateTime("0001-01-01T00:00:00.000+00:00")));
                }
                status = "Success";
                message = "Record Updated";
            }
            catch (Exception ex)
            {
                message = ex.ToString();
            }
            return Ok(new { status = status, message = message });
        }

        //Fuel Finish Time
        [HttpPost]
        //[Route("api/Fuel/UpdateFuelFinishTime")]
        public IHttpActionResult UpdateFuelFinishTime(FuelInfo fuelInfo)
        {
            var status = "Fail";
            string message = string.Empty;
            try
            {
                var collection = database.GetCollection<FuelInfo>("FuelInfo");
                collection.FindOneAndUpdate(x => x.FuelStation == fuelInfo.FuelStation && x.FuelType == fuelInfo.FuelType, new UpdateDefinitionBuilder<FuelInfo>().Set(x => x.FinishTime, fuelInfo.FinishTime));
                status = "Success";
                message = "Record Updated";
            }
            catch (Exception ex)
            {
                message = ex.ToString();

            }
            return Ok(new { status = status, message = message });


        }

    }
}
