using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using timeWork.Functions.Entities;

namespace timeWork.Functions.Functions
{
    public static class ScheduledFunction
    {
        [FunctionName("ScheduledFunction")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation($"Deleting completed function executed at:{DateTime.Now}");

            string filter = TableQuery.GenerateFilterConditionForBool("consolidado", QueryComparisons.Equal, true);
            TableQuery<timeEntity> query = new TableQuery<timeEntity>().Where(filter);
            TableQuerySegment<timeEntity> completedTimes = await timeTable.ExecuteQuerySegmentedAsync(query, null);
            int deleted = 0;
            foreach (timeEntity completedTime in completedTimes)
            {
                await timeTable.ExecuteAsync(TableOperation.Delete(completedTime));
                deleted++;
            }
            log.LogInformation($"Deleted: {deleted} items at: {DateTime.Now}");
        }
    }
}
