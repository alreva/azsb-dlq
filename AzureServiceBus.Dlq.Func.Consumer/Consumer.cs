using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureServiceBus.Dlq.Func.Consumer;

public class Consumer
{
    [FunctionName("Consumer")]
    public void Run(
        [ServiceBusTrigger("customers", Connection = "ServiceBusConnection")]
        string myQueueItem,
        ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
    }
}