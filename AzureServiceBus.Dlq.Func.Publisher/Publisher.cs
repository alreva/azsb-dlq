using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureServiceBus.Dlq.Func.Publisher;

public static class Publisher
{
    private static readonly ServiceBusClient client = new(
        Environment.GetEnvironmentVariable("ServiceBusConnection:fullyQualifiedNamespace"),
        new DefaultAzureCredential(),
        new ServiceBusClientOptions
        { 
            TransportType = ServiceBusTransportType.AmqpWebSockets
        });
    private static readonly ServiceBusSender sender = client.CreateSender("customers");

    [FunctionName("Publisher")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
        HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processing a request...");

        string name = req.Query["name"];

        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data = JsonConvert.DeserializeObject(requestBody);
        name = name ?? data?.name;

        if (string.IsNullOrWhiteSpace(name))
        {
            return new OkObjectResult(new
            {
                Errors = new [] {
                    new {
                        Status = "400",
                        Message = "This HTTP triggered function executed successfully." +
                                  " Pass a name in the query string or in the request body for a personalized response."
                    }
                }
            });
        }
        
        var responseMessage = await SendMessage(name);
        
        log.LogInformation(responseMessage);

        return new OkObjectResult(new
        {
            Data = responseMessage
        });
    }

    private static async Task<string> SendMessage(string name)
    {
        await sender.SendMessageAsync(new ServiceBusMessage(name));
        return $"Hello, {name}. This HTTP triggered function sent the message to the Azure queue";
    }
}