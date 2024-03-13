using System.Text;
using System.Text.Json;
using MQTTnet.AspNetCore;
using MQTTnet.Client;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;

namespace MQTTnet.Samples.Server
{
    public static class MQTTServer
    {
        public static async Task Start()
        {
            var storePath = Path.Combine(Path.GetTempPath(), "RetainedMessages.json");

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = mqttFactory
                .CreateServerOptionsBuilder()
                .WithDefaultEndpoint()
                .Build();

            using (var server = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                server.LoadingRetainedMessageAsync += async eventArgs =>
                {
                    try
                    {
                        var models =
                            await JsonSerializer.DeserializeAsync<List<MqttRetainedMessageModel>>(
                                File.OpenRead(storePath)
                            ) ?? new List<MqttRetainedMessageModel>();
                        var retainedMessages = models
                            .Select(m => m.ToApplicationMessage())
                            .ToList();

                        eventArgs.LoadedRetainedMessages = retainedMessages;
                        Console.WriteLine("Retained messages loaded.");
                    }
                    catch (FileNotFoundException)
                    {
                        Console.WriteLine("No retained messages stored yet.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                };

                server.RetainedMessageChangedAsync += async eventArgs =>
                {
                    try
                    {
                        var models = eventArgs.StoredRetainedMessages.Select(
                            MqttRetainedMessageModel.Create
                        );

                        var buffer = JsonSerializer.SerializeToUtf8Bytes(models);
                        await File.WriteAllBytesAsync(storePath, buffer);
                        Console.WriteLine("Retained messages saved.");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                };

                server.RetainedMessagesClearedAsync += _ =>
                {
                    File.Delete(storePath);
                    return Task.CompletedTask;
                };



                var host = Host.CreateDefaultBuilder(Array.Empty<string>())
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseKestrel(o =>
                        {
                            o.ListenAnyIP(1883, l => l.UseMqtt());
                            o.ListenAnyIP(5000); // Default HTTP pipeline
                        });

                        webBuilder.UseStartup<MqttServerStartup>();
                    });

                await server.StartAsync();
                await host.RunConsoleAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();
            }
        }

        sealed class MqttController
        {
            public MqttController()
            {
                // Inject other services via constructor.
            }

            public Task OnClientConnected(ClientConnectedEventArgs eventArgs)
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
                return Task.CompletedTask;
            }

            public Task ValidateConnection(ValidatingConnectionEventArgs eventArgs)
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' wants to connect. Accepting!");
                return Task.CompletedTask;
            }

            public Task OnClientDisconnected(ClientDisconnectedEventArgs eventArgs)
            {
                Console.WriteLine($"Client '{eventArgs.ClientId}' disconnected.");
                return Task.CompletedTask;
            }
            public Task OnApplicationMessageReceived(InterceptingPublishEventArgs eventArgs)
            {
                Console.WriteLine($"Message received from client '{eventArgs.ClientId}'. Topic: {eventArgs.ApplicationMessage.Topic}. Payload: {Encoding.UTF8.GetString(eventArgs.ApplicationMessage.PayloadSegment.ToArray())}");
                return Task.CompletedTask;
            }
        }

        sealed class MqttServerStartup
        {
            public void Configure(
                IApplicationBuilder app,
                IWebHostEnvironment environment,
                MqttController mqttController
            )
            {
                app.UseRouting();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapConnectionHandler<MqttConnectionHandler>(
                        "/mqtt",
                        httpConnectionDispatcherOptions =>
                            httpConnectionDispatcherOptions.WebSockets.SubProtocolSelector =
                                protocolList => protocolList.FirstOrDefault() ?? string.Empty
                    );
                });

                app.UseMqttServer(server =>
                {
                    server.ValidatingConnectionAsync += mqttController.ValidateConnection;
                    server.ClientConnectedAsync += mqttController.OnClientConnected;
                    server.ClientDisconnectedAsync += mqttController.OnClientDisconnected;
                    server.InterceptingPublishAsync += mqttController.OnApplicationMessageReceived;


                });
            }

            public void ConfigureServices(IServiceCollection services)
            {
                services.AddHostedMqttServer(optionsBuilder =>
                {
                    optionsBuilder.WithDefaultEndpoint();
                });

                services.AddMqttConnectionHandler();
                services.AddConnections();

                services.AddSingleton<MqttController>();
            }
        }

        sealed class MqttRetainedMessageModel
        {
            public string? ContentType { get; set; }
            public byte[]? CorrelationData { get; set; }
            public byte[]? Payload { get; set; }
            public MqttPayloadFormatIndicator PayloadFormatIndicator { get; set; }
            public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; }
            public string? ResponseTopic { get; set; }
            public string? Topic { get; set; }
            public List<MqttUserProperty>? UserProperties { get; set; }

            public static MqttRetainedMessageModel Create(MqttApplicationMessage message)
            {
                if (message == null)
                {
                    throw new ArgumentNullException(nameof(message));
                }

                return new MqttRetainedMessageModel
                {
                    Topic = message.Topic,
                    Payload = message.PayloadSegment.ToArray(),
                    UserProperties = message.UserProperties,
                    ResponseTopic = message.ResponseTopic,
                    CorrelationData = message.CorrelationData,
                    ContentType = message.ContentType,
                    PayloadFormatIndicator = message.PayloadFormatIndicator,
                    QualityOfServiceLevel = message.QualityOfServiceLevel
                };
            }

            public MqttApplicationMessage ToApplicationMessage()
            {
                return new MqttApplicationMessage
                {
                    Topic = Topic,
                    PayloadSegment = new ArraySegment<byte>(Payload ?? Array.Empty<byte>()),
                    PayloadFormatIndicator = PayloadFormatIndicator,
                    ResponseTopic = ResponseTopic,
                    CorrelationData = CorrelationData,
                    ContentType = ContentType,
                    UserProperties = UserProperties,
                    QualityOfServiceLevel = QualityOfServiceLevel,
                    Dup = false,
                    Retain = true
                };
            }
        }
    }
}
