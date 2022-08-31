using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSystemsManager(
    source => {
                source.Path = "/";
                source.AwsOptions = new AWSOptions() 
                {
                    Region = RegionEndpoint.USWest2
                };
                source.ReloadAfter = TimeSpan.FromSeconds(30);
            });

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.Map("api/meteorites", (IConfiguration config) => {
    var secret = config["DynamoDBConnectionString"];
    return secret;

});

app.Map("api/weather", async (string city, IHttpClientFactory factory, IConfiguration config) => {

    var httpClient = factory.CreateClient();
    var apiKey = config["ApiKey"];
        
    var requestUri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
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
