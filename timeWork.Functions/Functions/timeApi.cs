using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using timeWork.Common.Models;
using timeWork.Common.Responses;
using timeWork.Functions.Entities;

namespace timeWork.Functions.Functions
{
    public static class timeApi
    {
        [FunctionName(nameof(CreateTime))]
        public static async Task<IActionResult> CreateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Received a new time");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);

            if (string.IsNullOrEmpty(time?.id))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a ID."
                });
            }

            timeEntity timeEntity = new timeEntity
            {
                CreatedTime = DateTime.UtcNow,
                ETag = "*",
                consolidado = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                tipo = 1,
                id = time.id
            };

            TableOperation addOperation = TableOperation.Insert(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = "New time stored in table";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity

            });
        }


        [FunctionName(nameof(UpdateTime))]
        public static async Task<IActionResult> UpdateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "time/{id}")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for time: {id}, received.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);

         
            TableOperation findOperation = TableOperation.Retrieve<timeEntity>("TIME", id);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "id not found."
                });
            }

            //Update todo

            timeEntity timeEntity = (timeEntity)findResult.Result;
            timeEntity.tipo = time.tipo;
            timeEntity.consolidado = time.consolidado;
            if (!string.IsNullOrEmpty(time.id))
            {
                timeEntity.id = time.id;
            }


            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);

            string message = $"id: {id}, update in table.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity

            });
        }


        [FunctionName(nameof(GetAllTimes))]
        public static async Task<IActionResult> GetAllTimes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Get all ids received.");

            TableQuery<timeEntity> query = new TableQuery<timeEntity>();
            TableQuerySegment<timeEntity> times = await timeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all times.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = times

            });
        }


        [FunctionName(nameof(GetTimeByID))]
        public static IActionResult GetTimeByID(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time/{id}")] HttpRequest req,
            [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] timeEntity timeEntity,
            string id,
            ILogger log)
            {

                log.LogInformation($"Get time by id: {id}, received.");

                if (timeEntity == null)
                {
                    return new BadRequestObjectResult(new Response
                    {
                        IsSuccess = false,
                        Message = "Time not found."
                    });
                }


                string message = $"Time: {timeEntity.RowKey}, retrieved.";
                log.LogInformation(message);


                return new OkObjectResult(new Response
                {
                    IsSuccess = true,
                    Message = message,
                    Result = timeEntity

                });
            }

        [FunctionName(nameof(DeleteTime))]
        public static async Task<IActionResult> DeleteTime(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "time/{id}")] HttpRequest req,
        [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] timeEntity timeEntity,
        [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
        string id,
        ILogger log)
        {

            log.LogInformation($"Delete time id: {id}, received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."
                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(timeEntity));
            string message = $"Time: {timeEntity.RowKey}, deleted.";
            log.LogInformation(message);


            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity

            });
        }
    }
}
