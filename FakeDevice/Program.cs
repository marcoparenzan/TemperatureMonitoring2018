using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using static System.Console;

namespace FakeDevice
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var devicetype = args[0];
            var hostname = args[1];
            var deviceId = args[2];
            var symmetricKey = args[3];

            WriteLine($"Hostname={hostname} deviceId={deviceId} devicetype={devicetype} symmetrickey={symmetricKey}");

            var random = new Random();
            var authentication = new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, symmetricKey);

            var client = DeviceClient.Create(hostname, authentication, TransportType.Mqtt);
            var twin = await client.GetTwinAsync();


            Update update = default(Update);
            var updateFilename = $"{deviceId}_update.json";
            if (File.Exists(updateFilename))
            {
                var json = File.ReadAllText(updateFilename);
                update = JsonConvert.DeserializeObject<Update>(json);
            }
            else
            {
                update = new Update();
                if (!twin.Properties.Reported.Contains("version"))
                {
                    var tc = new TwinCollection();
                    tc["version"] = update.Version;
                    await client.UpdateReportedPropertiesAsync(tc);
                }
            }

            var state = "";
            int min, max, threshold;
            ResetThreshold(devicetype, out threshold);
            if (twin.Properties.Desired.Contains("desiredValue"))
            {
                PatchDesiredValue(twin.Properties.Desired, out min, out max);
            }
            else
            {
                ResetDesiredValue(devicetype, out min, out max);
            }

            await client.SetDesiredPropertyUpdateCallbackAsync(async (tc, content) =>
            {
                PatchDesiredValue(tc, out min, out max);
            }, null);

            await client.SetMethodHandlerAsync("update", async (req, context) =>
            {
                var json = Encoding.UTF8.GetString(req.Data);
                WriteLine($"Update with:");
                WriteLine(json);
                try
                {
                    File.WriteAllText(updateFilename, json);
                    update = JsonConvert.DeserializeObject<Update>(json);
                    
                    // update version
                    var tc = new TwinCollection();
                    tc["version"] = update.Version;
                    await client.UpdateReportedPropertiesAsync(tc);
                    WriteLine("Update done!");
                }
                catch
                {
                }
                return null;
            }, null);

            await client.SetMethodHandlerAsync("reset", (req, context) =>
            {
                WriteLine($"Reset parameters...");
                ResetDesiredValue(devicetype, out min, out max);
                return null;
            }, null);

            // receive
            Task.Run(async () =>
            {
                while (true)
                {
                    Message message = default(Message);
                    try
                    {
                        message = await client.ReceiveAsync();

                        var bytes = message.GetBytes();
                        var text = Encoding.UTF8.GetString(bytes);
                        WriteLine($"Received message>>{text}");

                        await client.CompleteAsync(message);
                    }
                    catch
                    {
                        await client.AbandonAsync(message);
                    }
                }
            });

            Task.Run(async () =>
            {
                while (true)
                {
                    var obj = new
                    {
                        Id = Guid.NewGuid(),
                        DeviceId = deviceId,
                        DeviceType = devicetype,
                        Value = random.Next(min, max)
                    };
                    var json = JsonConvert.SerializeObject(obj);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var message = new Message(bytes);
                    await client.SendEventAsync(message);
                    WriteLine($"Sending {json}...");

                    //var newState = obj.Value > threshold ? "alarm" : "normal";
                    var newState = await update.AlarmAsync(obj.Value, threshold);
                    if (newState != state)
                    {
                        state = newState;
                        WriteLine($"New state {state}...");
                        var tc = new TwinCollection();
                        tc["state"] = state;
                        await client.UpdateReportedPropertiesAsync(tc);
                    }

                    await Task.Delay(5000);
                }
            });

            WriteLine($"Running {devicetype} {deviceId}");
            var stay = true;
            while (stay)
            {
                var key = ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.OemPlus:
                        min += 2;
                        max = min + 10;
                        WriteLine($"Min={min} Max={max}");
                        break;
                    case ConsoleKey.OemMinus:
                        min -= 2;
                        max = min + 10;
                        WriteLine($"Min={min} Max={max}");
                        break;
                    case ConsoleKey.Escape:
                        stay = false;
                        break;
                }
            }
        }

        private static void PatchDesiredValue(TwinCollection tc, out int min, out int max)
        {
            var desiredValue = (int)tc["desiredValue"];
            min = desiredValue - 5;
            max = min + 10;
            WriteLine($"Setting desiredvalue: Min={min} Max={max}");
        }

        private static void ResetDesiredValue(string devicetype, out int min, out int max)
        {
            min = -25;
            max = -15;
            if (devicetype == "humidity")
            {
                min = 10;
                max = 20;
            }
            WriteLine($"Min={min} Max={max}");
        }

        private static void ResetThreshold(string devicetype, out int threshold)
        {
            threshold = 0;
            if (devicetype == "humidity")
            {
                threshold = 30;
            }
            WriteLine($"threshold={threshold}");
        }
    }
}
