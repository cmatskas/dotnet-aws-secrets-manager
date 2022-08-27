using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IAmazonSecretsManager>(a =>
      new AmazonSecretsManagerClient(RegionEndpoint.USWest2)
);
builder.Services.AddHttpClient();

var app = builder.Build();

app.Map("api/meteorites", async (IAmazonSecretsManager secretsManager) => {
    var secret = await secretsManager.GetSecretValueAsync(
        new GetSecretValueRequest()
        {
            SecretId = "DynamoDBConnectionString",
        }
    );
    return secret.SecretString;

});

app.Map("api/weather", async (string city, IHttpClientFactory factory, IAmazonSecretsManager secretsManager) => {

    var httpClient = factory.CreateClient();
    var apiKey = await secretsManager.GetSecretValueAsync(
        new GetSecretValueRequest()
        {
            SecretId = "ApiKey"
        }
    );

    var requestUri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey.SecretString}";
    var response = await httpClient.GetAsync(requestUri);
    return await response.Content.ReadAsStringAsync();
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
