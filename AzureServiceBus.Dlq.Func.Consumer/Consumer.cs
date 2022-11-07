using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace AzureServiceBus.Dlq.Func.Consumer;

public class Consumer
{
    [ServiceBusAccount("ServiceBusConnection")]
    [FunctionName("Consumer")]
    public async Task Run(
        [ServiceBusTrigger("customers")]
        string message,
        ILogger log)
    {
        log.LogInformation($"C# ServiceBus queue trigger function processed message: {message}");
    }
}