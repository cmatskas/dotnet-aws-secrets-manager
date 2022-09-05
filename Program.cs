using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.SimpleSystemsManagement.Model;
using Amazon.SimpleSystemsManagement;
using Amazon.DynamoDBv2.Model;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddSystemsManager(
    source => {
                source.Path = "/";
                source.AwsOptions = new AWSOptions() 
                {
                    Region = RegionEndpoint.USEast1
                };
                source.ReloadAfter = TimeSpan.FromSeconds(30);
            });

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

var app = builder.Build();

app.Map("api/meteorites", async (string name, IConfiguration config) => {

    return await GetMeteoritesAsync(name);
});

app.MapPost("api/meteorites", async(Meteorite meteorite) => {
    return await CreateMeteoriteRecord(meteorite);
});

app.Map("api/weather", async (string city, IHttpClientFactory factory, IConfiguration config) => {
    var httpClient = factory.CreateClient();
    var apiKey = config["ApiKey"];
        
    var requestUri = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}";
    var response = await httpClient.GetAsync(requestUri);
    return await response.Content.ReadAsStringAsync();
});

app.Map("api/getsecret", async (string name) => {
    return await GetValueFromParameterStore(name);
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

async Task<IEnumerable<Meteorite>> GetMeteoritesAsync(string name)
{
    var client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
    var context = new DynamoDBContext(client);
    var scs = new List<ScanCondition>();
    var sc = new ScanCondition("Name", ScanOperator.Contains, name);

    scs.Add(sc);

    var cfg = new DynamoDBOperationConfig
    {
        QueryFilter = scs,
    };

    var response = context.ScanAsync<Meteorite>(scs);
    return await response.GetRemainingAsync();
}

async Task<PutItemResponse> CreateMeteoriteRecord(Meteorite meteorite)
{
    var client = new AmazonDynamoDBClient(RegionEndpoint.USEast1);
    var newMeteorite = ConvertMeteoriteToDBDocument(meteorite);
    var result = await client.PutItemAsync("Meteorites", newMeteorite);

    return result;
}

async Task<string> GetValueFromParameterStore(string paramName)
{
    var client = new AmazonSimpleSystemsManagementClient(RegionEndpoint.USEast1);
    var request = new GetParameterRequest()
    {
        Name = paramName,
        WithDecryption = true,
    };

    var result = await client.GetParameterAsync(request);
    return result.Parameter.Value;
}

Dictionary<string, AttributeValue> ConvertMeteoriteToDBDocument(Meteorite meteorite)
{
    var propertyDictionary = new Dictionary<string, AttributeValue>();
    foreach(var prop in meteorite.GetType().GetProperties())
    {
        propertyDictionary.Add(prop.Name, new AttributeValue(prop.GetValue(meteorite, null).ToString()));
    }

    return propertyDictionary;
}
