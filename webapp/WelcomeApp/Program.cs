using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var credential = new DefaultAzureCredential();

app.MapGet("/", async () =>
{
    // 1. Read the secret from Key Vault
    var secretClient = new SecretClient(
        new Uri("https://learningkeyvault-83140.vault.azure.net/"),
        credential);
    KeyVaultSecret secret = await secretClient.GetSecretAsync("welcome-message");

    // 2. Send a message to the Service Bus queue
    await using var serviceBusClient = new ServiceBusClient(
        "learning-dev-servbus-83140.servicebus.windows.net",
        credential);
    ServiceBusSender sender = serviceBusClient.CreateSender("orders");
    await sender.SendMessageAsync(new ServiceBusMessage($"Order triggered: {secret.Value}"));

    return Results.Ok(new { message = secret.Value, status = "Message sent to queue" });
});

app.Run();