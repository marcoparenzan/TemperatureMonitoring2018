#r "Newtonsoft.Json"
#r "Microsoft.Azure.WebJobs.Extensions"
#r "Microsoft.WindowsAzure.Storage"
#r "Microsoft.ServiceBus"

using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Microsoft.Azure.WebJobs;

using Microsoft.ServiceBus.Messaging;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

public class Read
{
    public string Id { get; set; }
    public string TenantId { get; set; }
    public string DeviceId { get; set; }
    public string DeviceType { get; set; }
    public int Value { get; set; }
}

public async static Task Run(Read myIoTHubMessage, Binder binder, TraceWriter log, ExecutionContext context)
{
    var data = new {
        deviceId = myIoTHubMessage.DeviceId,
        location = ConfigurationManager.AppSettings["location"],
        deviceType = myIoTHubMessage.DeviceType,
        value = myIoTHubMessage.Value
    };
    var json = JsonConvert.SerializeObject(data);

    if (myIoTHubMessage.Value>0 && myIoTHubMessage.DeviceType == "temperature")
    {
        log.Info($"temperature alert: deviceId={myIoTHubMessage.DeviceId}, deviceType={myIoTHubMessage.DeviceType}, value={myIoTHubMessage.Value}");

        var httpClient = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await httpClient.PostAsync(ConfigurationManager.AppSettings["AlertNotificationUrl"], content);
    }
    else if (myIoTHubMessage.Value>30 && myIoTHubMessage.DeviceType == "humidity")
    {
        log.Info($"humidity alert: deviceId={myIoTHubMessage.DeviceId}, deviceType={myIoTHubMessage.DeviceType}, value={myIoTHubMessage.Value}");

        var httpClient = new HttpClient();
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await httpClient.PostAsync(ConfigurationManager.AppSettings["AlertNotificationUrl"], content);
    }
    else
    {
    }

    var storageConnection = "tm2018_STORAGE";
    var hubPath = $"devicestates/{data.location}_{data.deviceId}.json";
 
    CloudBlockBlob hubBlob = null;
    hubBlob = await binder.BindAsync<CloudBlockBlob>(new Attribute[]
    {
        new BlobAttribute(hubPath.ToLower()),
        new StorageAccountAttribute(storageConnection)
    }).ConfigureAwait(false);

    await hubBlob.UploadTextAsync(json);
}